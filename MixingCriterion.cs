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
        [CustomFunction("Stratification", "Calculate mixing criterion from Bormans and Webster (1997).\nInputs are flow (ML/d), surface area (ha), storage volume (ML), and length (m) of reach.\nUnits must match above, not handled automatically\n45,000 is the treshold to disrupt stratification")]
        public static double Stratification(double flow,double area,double volume,double length)
        {
            double Qnet = 169.435868; //total raditation, W/m2
            double Qi = 371; //shortwave radiation, W/m2 CURRENTLY Qnet and Qi are hard coded to Renmark in December. Needs to be made dynamic, use pattern or something
            double kd = 2.5; //light attenuation, /m
            double a = 0.00021; //thermal expansion coefficient
            double g = 9.81; //gravity, m/s/s
            double p = 998; //density of water, kg/m3
            double Cp = 4180; //specific heat of water J/kgC
            
            //convert units to SI
            flow = flow / 86.4;
            area = area * 10000.0;
            volume = volume * 1000.0;

            //reach averaged velocity and depth
            double velocity = flow / (volume / length);
            double depth = volume / area;

            double R;

            R = Math.Pow(velocity, 3) / (depth * (Qnet - (2.0 * Qi / (kd * depth))) * (a * g) / (p * Cp));

            return R;
        }
    }
}
