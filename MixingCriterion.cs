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
        public static double Stratification(double flow, double WSL, double volume, double length, double bottom_el, double Qnet, double Qi)
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

        [CustomFunction("StratificationRisk", "Calculate mixing criterion from Bormans and Webster (1997).\nInputs are velocity (m/s), water surface elevation (m AHD), bottom elevation (m AHD) of reach, net shortwave radiation (W/m2), Water Temp (degrees), Air Temp (degrees), Relative Humidity (%) and Wind Speed (m/s).\nUnits must match above, not handled automatically\nDepth needs to be a \"reach average\". It is suggested to determine this using WL_NPL_m-vol_NPL_m3/area_NPL_m2\n45,000 is the treshold to disrupt stratification.\nVelocity can be calcuated using AverageVelocity()")]
        public static double StratificationRisk(double velocity, double WSL, double bottom_el, double Q_swnet, double WaterTemp, double AirTemp, double RH, double WindSpeed)
        {
            //Code reproduces the results from:
            //Critical flow estimation to determine cyanobacterial risk following mass mortality of carp following release of proposed CyHV - 3 carp biocontrol
            //Richard Walsh, Justin Brookes, Sanjina Upadhyay
            //University of Adelaide 2018

            double kd = 2.5; //light attenuation, /m
            double a = 0.00021; //thermal expansion coefficient
            double g = 9.81; //gravity, m/s/s
            double p = 998; //density of water, kg/m3
            double Cp = 4180; //specific heat of water J/kgC
            double Cp_a = 1010; //specific heat capacity of air J/KgC
            double emissivity_w = 0.972; //emissivity of water 
            double sigma = 5.7e-8; //Stefan-Boltzman constant
            double Lv = 2.5e6; //Latent head of evaporation
            double pa = 1.2; //kg/m3 density of air
            double CE = 3e-3; //coefficicent from Bormans and Webster (1997)
            double CH = 2e-3; //coefficicent from Bormans and Webster (1997)

            double depth = WSL - bottom_el;

            //Qb - net uward long wave radiation
            double emittedLW = -emissivity_w * sigma * (Math.Pow(273.2 + WaterTemp, 4));
            double emissivity_a = 0.0000092 * (Math.Pow(273.2 + AirTemp, 2));
            double absorbedLW = emissivity_a * sigma * (1 + 0.17 * (Math.Pow(0, 2))) * Math.Pow(273.2 + AirTemp, 4) * (1 - 0.3); //CHECK THIS - 0^2???
            double Qb = absorbedLW + emittedLW;

            //Qe - heat flux of evaporation
            double ea = RH / 100 * 610.6 * Math.Exp((17.27 * AirTemp) / (AirTemp + 237.3));
            double es = 610.6 * Math.Exp((17.27 * WaterTemp) / (WaterTemp + 237.3));
            double qa = ea * 0.622 / 100000;
            double qs = es * 0.622 / 100000;
            double W10 = (WindSpeed * (Math.Log(10 / 0.000115))) / (Math.Log(2 / 0.000115));
            double Qe = Lv * pa * CE * W10 * (qs - qa);

            //Qs - upward sensible head flux
            double Qs = Cp_a * pa * CH * W10 * (WaterTemp - AirTemp);

            //net surface heat flux
            double Qnet = Qs + Qe + Qb + Q_swnet;

            double R;

            R = Math.Pow(velocity, 3) / (depth * (Qnet - (2.0 * Q_swnet / (kd * depth))) * (a * g) / (p * Cp));

            return R;
        }

        [CustomFunction("AverageVelocity", "Calculate velocity as v=Q/A, where A=volume/length. \nInputs are flow (ML/d),  storage volume (ML), length of reach (m)")]
        public static double AverageVelocity(double flow, double volume, double length)
        {
            //convert units to SI - is there a type that provides access to units, rather than double?
            double flowSI = flow / 86.4;
            double volumeSI = volume * 1000.0;

            //reach averaged velocity and depth
            double velocity = flowSI / (volumeSI / length);

            return velocity;
        }
    }
}
