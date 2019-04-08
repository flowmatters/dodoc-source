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
        [CustomFunction("Stratification", "Calculate mixing criterion from Bormans and Webster (1997).\nInputs are flow (ML/d), water surface elevation (m AHD), storage volume (ML), length of reach (m), bottom elevation (m AHD) of reach, net longwave radiation (W/m2) and shortwave radiation (W/m2).\nUnits must match above, not handled automatically\nDepth needs to be a \"reach average\". It is suggested to determine this using WL_NPL_m-vol_NPL_m3/area_NPL_m2\n45,000 is the treshold to disrupt stratification")]
        public static double Stratification(double flow,double WSL,double volume,double length,double bottom_el,double Qnet,double Qi)
        {
            //double Qnet = 169.435868; //total raditation, W/m2
            //double Qi = 371; //shortwave radiation, W/m2 CURRENTLY Qnet and Qi are hard coded to Renmark in December. Needs to be made dynamic, use pattern or something
            double kd = 2.5; //light attenuation, /m
            double a = 0.00021; //thermal expansion coefficient
            double g = 9.81; //gravity, m/s/s
            double p = 998; //density of water, kg/m3
            double Cp = 4180; //specific heat of water J/kgC
            
            //convert units to SI - is there a type that provides access to units, rather than double?
            double flowSI = flow / 86.4;
            double volumeSI = volume * 1000.0;

            //reach averaged velocity and depth
            double velocity = flowSI / (volumeSI / length);
            double depth = WSL - bottom_el;

            double R;

            R = Math.Pow(velocity, 3) / (depth * (Qnet - (2.0 * Qi / (kd * depth))) * (a * g) / (p * Cp));

            return R;
        }
    }
}
