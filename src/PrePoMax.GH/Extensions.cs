using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CaeModel;
using CaeGlobals;
using FileInOut.Output.Calculix;
using FileInOut.Output;

namespace PrePoMax.GH
{
    public class Extensions
    {

    }

    [Serializable]
    public class EngineeringConstants : MaterialProperty
    {
        // Variables
        private double[] _youngsModuli;
        private double[] _poissonsRatios;
        private double[] _shearModuli;


        // Properties                                                                                                               
        public double[] YoungsModuli
        {
            get { return _youngsModuli; }
            set { if (value != null) _youngsModuli = value; else throw new CaeGlobals.CaeException(_positive); }
        }
        public double[] PoissonsRatios
        {
            get { return _poissonsRatios; }
            set { _poissonsRatios = value; }
        }
        public double[] ShearModuli
        {
            get { return _shearModuli; }
            set { _shearModuli = value; }
        }


        // Constructors                                                                                                             
        public EngineeringConstants(double[] youngsModuli, double[] poissonsRatios, double[] shearModuli)
        {
            // The constructor must wotk with E = 0
            _youngsModuli = youngsModuli;
            // Use the method to perform any checks necessary
            PoissonsRatios = poissonsRatios;
            // Use the method to perform any checks necessary
            ShearModuli = shearModuli;
        }

        // Methods
        // 

        public string GetDataString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("*Elastic, TYPE=ENGINEERING CONSTANTS{0}", Environment.NewLine);
            sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}{9},{10}{11}",
                _youngsModuli[0].ToCalculiX16String(),
                _youngsModuli[1].ToCalculiX16String(),
                _youngsModuli[2].ToCalculiX16String(),
                _poissonsRatios[0].ToCalculiX16String(),
                _poissonsRatios[1].ToCalculiX16String(),
                _poissonsRatios[2].ToCalculiX16String(),
                _shearModuli[0].ToCalculiX16String(),
                _shearModuli[1].ToCalculiX16String(),
                _shearModuli[2].ToCalculiX16String(),
                20.0.ToCalculiX16String(), Environment.NewLine);

            return sb.ToString();
        }
    }

    [Serializable]
    internal class CalEngineeringConstants : CalculixKeyword
    {
        private EngineeringConstants _eConstants;
        private bool _temperatureDependent;

        public CalEngineeringConstants(EngineeringConstants eConstants, bool temperatureDependent)
        {
            _eConstants = eConstants;
            _temperatureDependent = temperatureDependent;
        }

        // Methods                                                                                                                  
        public override string GetKeywordString()
        {
            return string.Format("*Elastic, TYPE=ENGINEERING CONSTANTS{0}", Environment.NewLine);
        }
        public override string GetDataString()
        {
            StringBuilder sb = new StringBuilder();
            var ym = _eConstants.YoungsModuli;
            var pr = _eConstants.PoissonsRatios;
            var sm = _eConstants.ShearModuli;

            sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}{9},{10}{11}",
                ym[0].ToCalculiX16String(),
                ym[1].ToCalculiX16String(),
                ym[2].ToCalculiX16String(),
                pr[0].ToCalculiX16String(),
                pr[1].ToCalculiX16String(),
                pr[2].ToCalculiX16String(),
                sm[0].ToCalculiX16String(),
                sm[1].ToCalculiX16String(),
                sm[2].ToCalculiX16String(),
                20.0.ToCalculiX16String(), Environment.NewLine);

            return sb.ToString();
        }

    }
}
