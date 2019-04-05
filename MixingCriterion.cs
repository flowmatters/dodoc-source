using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RiverSystem.TaskDefinitions;

namespace CustomFunctions
{
    [CustomFunctionContainer]
    public class MixingCriterion
    {
        [CustomFunction("Stratification", "Calculate mixing criterion from Bormans and Webster (1997).\nInputs are flow, surface area, storage volume, and length of reach.\nMust be in SI units (m, m3/s)\n45,000 is the treshold to disrupt stratification")]
        public static double Stratification(double flow,double area,double volume,double length)
        {
            double Qnet = 142.3269; //total raditation, W/m2
            double Qi = 321; //shortwave radiation, W/m2
            double kd = 2.5; //light attenuation, /m
            double a = 0.00021; //thermal expansion coefficient
            double g = 9.81; //gravity, m/s/s
            double p = 998; //density of water, kg/m3
            double Cp = 4180; //specific heat of water J/kgC


            //no checking of units, need to be in SI (m, m3/s)
            double velocity = flow / (volume / length);
            double depth = volume / area;

            double R;

            R = Math.Pow(velocity, 3) / (depth * (Qnet - (2.0 * Qi / (kd * depth))) * (a * g) / (p * Cp));

            return R;
        }
    }
}
