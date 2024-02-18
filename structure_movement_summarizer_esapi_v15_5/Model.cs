using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace structure_movement_summarizer_esapi_v15_5.Models
{

    internal class Model
    {

        public Dictionary<string, Int32> StructureNumOfPhasesPairs { get; private set; } = new Dictionary<string, int>();
        public Int32 NumOfPhases { get; private set; } = 0;
        public ReactiveCollection<string> PlottableStructureId { get; private set; } = new ReactiveCollection<string>();
        public PhaseImagesArray PhaseArray { get; set; } = new PhaseImagesArray();
        public PhaseImages _TmpImage { get; } = new PhaseImages();
        public List<StructureSet> StSetList { get; set; } = new List<StructureSet>();
        private ScriptContext Context { get; set; }

        public Model()
        {

        }

        public void SetScriptContext( ScriptContext _context)
        {
            Context = _context;

            var frame_of_ref = Context.Image.FOR;
            foreach (var ss in Context.Patient.StructureSets)
            {
//                if (ss.Image.FOR == frame_of_ref)
                if ((ss.Image.FOR == frame_of_ref) && 
                    (_TmpImage.ParsePhaseComment(ss.Image.Series.Comment).Item1 == true))
                {
                    StSetList.Add(ss);
                }
                else { }
            }

            // count the number of phases witch a Structure Id is had.
            foreach (var ss in StSetList)
            {
                foreach (var st in ss.Structures)
                {
                    if (StructureNumOfPhasesPairs.TryGetValue(st.Id, out Int32 count))
                    {
                        StructureNumOfPhasesPairs[st.Id] = ++count;
                    }
                    else
                    {
                        StructureNumOfPhasesPairs.Add(st.Id, 1);
                    }
                }


                // count the number of phases except MIP or Ave
                if ((ss.Image.Series.Comment.Contains("MIP") == false) && (ss.Image.Series.Comment.Contains("Ave") == false)) {
                    ++NumOfPhases;
                }
                else { }

            }


            // if all of phases except MIP or Ave have same Structure Id, it can be plotted.
            foreach (var elem in StructureNumOfPhasesPairs)
            {
//                if (elem.Value >= NumOfPhases)
                if (elem.Value >= 10)
                {
                    PlottableStructureId.Add(elem.Key);
                }
                else { }
            }

        }

        public string ConvertStructureSetArrayToPhaseArray(in string structure_name)
        {
            string log = "";
            string stname = structure_name;

            // to sort in order to the Phase (%), 
            List<PhaseImages> phases = new List<PhaseImages>();

            foreach (var ss in StSetList)
            {
                if (ss.Structures.Any(x => x.Id == stname)) {
                    phases.Add(new PhaseImages(ss, structure_name));
                    log += $"Phases.Name: {phases.Last().Name}\t Phases.Phase: {phases.Last().Phase}\t ss.Id: {ss.Id}\t structure_name: {ss.Structures.FirstOrDefault(x => x.Id == stname).Id}\n";
                }
                else { }
            }
            phases.Sort((a, b) => {
                Double aval = (Double.TryParse(a.Phase, out var aphase)) ? aphase : Double.MaxValue;
                Double bval = (Double.TryParse(b.Phase, out var bphase)) ? bphase : Double.MaxValue;
                return aval.CompareTo(bval);
            });

            PhaseArray.Images.Clear();
            foreach (var phase in phases)
            {
                PhaseArray.Images.Add(phase);
            }

            return log;
        }
    }
}
