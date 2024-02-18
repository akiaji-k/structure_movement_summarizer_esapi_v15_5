using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using System.Windows;
using System.Linq;
using Reactive.Bindings.Extensions;
using System.Linq.Expressions;
using System.Numerics;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace structure_movement_summarizer_esapi_v15_5.Models
{
    public struct Coord
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
    public class PhaseImages : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private CompositeDisposable _disposable { get; } = new CompositeDisposable();

        public ReactiveProperty<bool> IsReference { get; set; } = new ReactiveProperty<bool>(false);
        public ReactiveProperty<bool> IsPlotted { get; set; } = new ReactiveProperty<bool>(true);
        public string Phase { get; set; }
        public string Name { get; set; }
        public Coord Pos3D { get; set; }
        public ReactivePropertySlim<Coord> PosDiff { get; set; }
        public  ReactivePropertySlim<double> DiffL2Norm { get; set; }

        public void Dispose() => _disposable.Dispose();

        public PhaseImages() { }
        public PhaseImages(in StructureSet ss, string structure_name) 
        {
            var ser = ss.Image.Series;
            IsReference = new ReactiveProperty<bool>((ParsePhaseComment(ser.Comment).Item2 == "0") ? true : false);
            IsPlotted.Value = (ser.Comment.Contains("MIP") | ser.Comment.Contains("Ave")) ? false : true;
            Phase = ParsePhaseComment(ser.Comment).Item2;
            Name = ss.Id;
            Pos3D = new Coord
            {
                X = ss.Structures.FirstOrDefault(x => x.Id == structure_name).CenterPoint.x,
                Y = ss.Structures.FirstOrDefault(x => x.Id == structure_name).CenterPoint.y,
                Z = ss.Structures.FirstOrDefault(x => x.Id == structure_name).CenterPoint.z,
            };
            PosDiff = new ReactivePropertySlim<Coord>(new Coord
            {
                X = 0.0,
                Y = 0.0,
                Z = 0.0,
            });
            DiffL2Norm = new ReactivePropertySlim<double>(0.0);
        }
        public (bool, string) ParsePhaseComment(in string comment)
        {
            bool is_phase = true;
            string phase = "null";

            if ((comment == null) || (comment == "")) { phase = "null"; is_phase = false; }
            else if (comment.Contains("T=0%,")) { phase = "0"; }
            else if (comment.Contains("T=5%,")) { phase = "5"; }
            else if (comment.Contains("T=10%,")) { phase = "10"; }
            else if (comment.Contains("T=15%,")) { phase = "15"; }
            else if (comment.Contains("T=20%,")) { phase = "20"; }
            else if (comment.Contains("T=25%,")) { phase = "25"; }
            else if (comment.Contains("T=30%,")) { phase = "30"; }
            else if (comment.Contains("T=35%,")) { phase = "35"; }
            else if (comment.Contains("T=40%,")) { phase = "40"; }
            else if (comment.Contains("T=45%,")) { phase = "45"; }
            else if (comment.Contains("T=50%,")) { phase = "50"; }
            else if (comment.Contains("T=55%,")) { phase = "55"; }
            else if (comment.Contains("T=60%,")) { phase = "60"; }
            else if (comment.Contains("T=65%,")) { phase = "65"; }
            else if (comment.Contains("T=70%,")) { phase = "70"; }
            else if (comment.Contains("T=75%,")) { phase = "75"; }
            else if (comment.Contains("T=80%,")) { phase = "80"; }
            else if (comment.Contains("T=85%,")) { phase = "85"; }
            else if (comment.Contains("T=90%,")) { phase = "90"; }
            else if (comment.Contains("T=95%,")) { phase = "95"; }
            else if (comment.Contains("MIP")) { phase = "MIP"; }
            else if (comment.Contains("Ave")) { phase = "Ave"; }
            else { phase = comment; is_phase = false; }

            return (is_phase, phase);
        }
        
    }

    public class PhaseImagesArray : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private CompositeDisposable _disposable { get; } = new CompositeDisposable();


        public event EventHandler<EventArgs> RePlot;

        public ReactiveCollection<PhaseImages> Images { get; set; } = new ReactiveCollection<PhaseImages>();

        public Int32 NumOfPhases { get; private set; } = 0;
        public string RefPhase { get; set; } = "0";
        public ReactiveCollection<double> phase_x { get; set; } = new ReactiveCollection<double>();
        public ReactiveCollection<double> amp_x { get; set; } = new ReactiveCollection<double>();
        public ReactiveCollection<double> amp_y { get; set; } = new ReactiveCollection<double>();
        public ReactiveCollection<double> amp_z { get; set; } = new ReactiveCollection<double>();

        public PhaseImagesArray()
        {
            Images = new ReactiveCollection<PhaseImages>();
            Images.ObserveAddChanged().Subscribe(x =>
                {
                    UpdateNumOfPhases();

                    x.IsReference.Subscribe(FindReferencePhase).AddTo(_disposable);

                    x.IsPlotted.Subscribe(_ =>
                    {
                        if (RePlot != null)
                        {
                            RePlot(this, EventArgs.Empty);
                        }
                        else { }
                    }).AddTo(_disposable);

                    x.PosDiff.Subscribe(_ =>
                    {
                        foreach (var image in Images)
                        {
                            image.DiffL2Norm.Value = Math.Sqrt(Math.Pow(image.PosDiff.Value.X, 2)
                                + Math.Pow(image.PosDiff.Value.Y, 2)
                                + Math.Pow(image.PosDiff.Value.Z, 2));

                        }
                        if (RePlot != null)
                        {
                            RePlot(this, EventArgs.Empty);
                        }
                        else { }
                    })
                    .AddTo(_disposable);
                }
            ).AddTo(_disposable);

        }


        public void FindReferencePhase(bool isChecked)
        {
            try
            {
                var ref_phase = Images.Where(x => x.IsReference.Value == true).First();
                if ((ref_phase != null) && (RefPhase != ref_phase.Phase))
                {
                    RefPhase = ref_phase.Phase;
//                    MessageBox.Show("Ref phase is " + RefPhase);
                }
            }
            catch { 
                // Reference となる要素がまだ ReactiveCollection に Add されていない場合、例外が発生される。
            }

            CalcDistFromRef();
        }

        public void CalcDistFromRef()
        {
            try
            {
                var ref_phase = Images.Where(x => x.Phase == RefPhase).First();
                if (ref_phase != null)
                {
                    foreach (var image in Images)
                    {
                        image.PosDiff.Value = new Coord
                        {
                            X = image.Pos3D.X - ref_phase.Pos3D.X,
                            Y = image.Pos3D.Y - ref_phase.Pos3D.Y,
                            Z = image.Pos3D.Z - ref_phase.Pos3D.Z,
                        };
                    }
                }
            }
            catch { 
                // Reference となる要素がまだ ReactiveCollection に Add されていない場合、例外が発生される。
            }

        }

        public (double[], double[], double[], double[], double[]) GetPosDiffArrays()
        {
            List<double> phase_x = new List<double>();
            List<double> amp_x = new List<double>();
            List<double> amp_y = new List<double>();
            List<double> amp_z = new List<double>();
            List<double> amp_norm = new List<double>();

            foreach (var image in Images)
            {
                if (image.IsPlotted.Value)
                {
                    try
                    {
                        phase_x.Add(Double.Parse(image.Phase));

                        amp_x.Add(image.PosDiff.Value.X);
                        amp_y.Add(image.PosDiff.Value.Y);
                        amp_z.Add(image.PosDiff.Value.Z);
                        amp_norm.Add(image.DiffL2Norm.Value);
                    }
                    catch { }
                }
                else { }
            }

            return (phase_x.ToArray(), amp_x.ToArray(), amp_y.ToArray(), amp_z.ToArray(), amp_norm.ToArray());
        }

        public void UpdateNumOfPhases()
        {
            Int32 _tmp = 0;
            foreach(var image in Images)
            {
                if ((image.Phase.Contains("MIP") == false) && (image.Phase.Contains("Ave") == false))
                {
                    _tmp++;
                }
                else { }
            }

            this.NumOfPhases = _tmp;
        }




    }
}
