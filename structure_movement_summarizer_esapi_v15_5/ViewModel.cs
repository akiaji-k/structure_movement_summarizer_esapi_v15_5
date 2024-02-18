using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Reactive.Disposables;
using structure_movement_summarizer_esapi_v15_5.Models;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Windows;
using Reactive.Bindings.Extensions;
using System.Numerics;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace structure_movement_summarizer_esapi_v15_5.ViewModels
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private CompositeDisposable _disposable { get; } = new CompositeDisposable();

        public ReactivePropertySlim<string> Id { get; } = new ReactivePropertySlim<string>("some ID");
        public ReactivePropertySlim<string> Name { get; } = new ReactivePropertySlim<string>("some Name");
        public ReactivePropertySlim<string> Date { get; } = new ReactivePropertySlim<string>("some Date");

        public Model InstModel { get; } = new Model();
        public ReactiveProperty<string> SelectedStructure { get; } = new ReactiveProperty<string>();



        public ViewModel() {

            SelectedStructure
                .Skip(1)
                .Subscribe(x =>
            {
                // debug
//                string buf = $"Selected: {x}, StSetList.Count(): {InstModel.StSetList.Count()}\n";
//                MessageBox.Show(buf);
                // debug end

                _ = InstModel.ConvertStructureSetArrayToPhaseArray(x);

                // debug
//                var log = InstModel.ConvertStructureSetArrayToPhaseArray(x);
//                MessageBox.Show(log);
                // debug end

            }).AddTo(_disposable);
        }

        internal void SetScriptContextToModel(in ScriptContext context)
        {
            InstModel.SetScriptContext(context);

            Id.Value = context.Patient.Id;
            Name.Value = context.Patient.FirstName + ", " + context.Patient.LastName;
            Date.Value = context.Image.Series.Study.CreationDateTime.ToString();

            // debug (Structure count)
//            string buf = "";
//            var numofphases = InstModel.NumOfPhases;
//            var dict = InstModel.StructureNumOfPhasesPairs;
//            buf += "NumOfPhases: " + numofphases;
//            foreach (var elem in dict)
//            {
//                buf += $"\n\t Name: {elem.Key}\tCount: {elem.Value}";
//            }
//            MessageBox.Show(buf);
        }

        public void Dispose() => _disposable.Dispose();
    }
}
