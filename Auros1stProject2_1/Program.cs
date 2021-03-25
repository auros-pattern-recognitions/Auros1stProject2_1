using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using static System.Math;

namespace Auros1stProject2_1
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //
            // "SiO2 1000nm_on_Si.dat" 파일 로딩 후
            // 측정 스펙트럼 데이터를 alpha, beta 로 변환,
            // "SiO2 1000nm_on_Si_new.dat" 파일로 저장한다.
            //
            // 2021.03.24 이지원.
            //
            #region psi, delta -> alpha, beta

            List<string> MeasurementSpectrumData = new List<string>();  // 측정 스펙트럼 데이터 저장할 배열. (한 줄씩 저장)
            string[] SingleLineData;                                    // 한 줄의 스펙트럼 데이터를 임시로 저장할 배열.

            // "SiO2 1000nm_on_Si.dat" 파일 읽기. (한 줄씩)
            MeasurementSpectrumData.AddRange(File.ReadAllLines("SiO2 1000nm_on_Si.dat"));

            // 무의미한 공백 행을 제거한다.
            int lenSpectrumData = MeasurementSpectrumData.Count;
            string Blank = "";
            for (int i = 0; i < lenSpectrumData; i++)
            {
                if (MeasurementSpectrumData[0] == Blank)
                    MeasurementSpectrumData.RemoveAt(0);
                else
                    break;
            }

            // wavelength : 350 ~ 980(nm)인 측정 스펙트럼 데이터를 담을 리스트 선언.
            List<double> wavelength_exp = new List<double>();   // 파장 데이터 리스트.
            List<double> AOI_exp        = new List<double>();   // 입사각 데이터 리스트.
            List<double> alpha_exp      = new List<double>();   // Psi 데이터 리스트.
            List<double> beta_exp       = new List<double>();   // Delta 데이터 리스트.

            // 데이터의 첫번째 줄은 column 명이다.
            // 이를 제외하기 위해 반복문을 1부터 시작한다.
            int StartIndex = 1;
            int LenData = MeasurementSpectrumData.Count;
            for (int i = StartIndex; i < LenData; i++)
            {
                // tsv 형식의 데이터를 SingleLineData에 저장한다.
                SingleLineData = MeasurementSpectrumData[i].Split((char)0x09);  // 0x09 : 수평 탭.
                // 파장이 350 ~ 980(nm) 이내인 데이터만 저장한다.
                if (Convert.ToDouble(SingleLineData[0]) >= 350.0 &&
                    Convert.ToDouble(SingleLineData[0]) <= 980.0)
                {
                    // 각 컬럼에 해당하는 데이터를 저장한다.
                    wavelength_exp.Add(Double.Parse(SingleLineData[0]));
                    AOI_exp.Add(Double.Parse(SingleLineData[1]));
                    alpha_exp.Add(Double.Parse(SingleLineData[2]));
                    beta_exp.Add(Double.Parse(SingleLineData[3]));
                }
            }

            // psi, delta -> alpha, beta 변환.

            // degree 를 radian 으로 변환해주는 함수.
            double degree2radian(double angle) => ((angle * (PI)) / 180);

            // Polarizer offset 각도. (45도)
            double PolarizerRadian = degree2radian(45.0);

            // psi, delta 데이터를 alpha, beta 로 변환한다.
            LenData = wavelength_exp.Count;
            for (int i = 0; i < LenData; i++)
            {
                // psi, delta 값을 radian 으로 변환한다.
                double PsiRadian = degree2radian(alpha_exp[i]); // ?
                double DeltaRadian = degree2radian(beta_exp[i]);

                // psi, delta 데이터를 alpha, beta 로 갱신한다.
                alpha_exp[i] = (
                    (Pow(Tan(PsiRadian), 2.0) - Pow(Tan(PolarizerRadian), 2.0))
                    / (Pow(Tan(PsiRadian), 2.0) + Pow(Tan(PolarizerRadian), 2.0)));
                beta_exp[i] = (
                    (2.0 * Tan(PsiRadian) * Tan(PolarizerRadian) * Cos(DeltaRadian))
                    / (Pow(Tan(PsiRadian), 2.0) + Pow(Tan(PolarizerRadian), 2.0)));
            }
            

            // 파일 쓰기.
            using (StreamWriter NewSpectrumOutputFile = new StreamWriter("SiO2 1000nm_on_Si_new.dat"))
            {
                // 컬럼 명 쓰기.
                NewSpectrumOutputFile.WriteLine(
                    "wavelength(nm)"    + "\t"
                    + "AOI"             + "\t"
                    + "alpha"           + "\t"
                    + "beta");

                // 스펙트럼 데이터 쓰기.
                for (int i = 0; i < LenData; i++)
                {
                    // tsv 데이터 형식으로 데이터를 쓴다.
                    NewSpectrumOutputFile.WriteLine(
                        wavelength_exp[i]   + "\t"
                        + AOI_exp[i]        + "\t"
                        + alpha_exp[i]      + "\t"
                        + beta_exp[i]);
                }
            }
            #endregion

            //
            // "Si_new.txt", "SiO2_new.txt" 파일 물성값 로딩.
            //
            // 2021.03.24 이지원.
            //
            #region MyRegion

            // "Si_new.txt" 파일 읽기.
            string[] Si_new = File.ReadAllLines("Si_new.txt");  // Si 기판 물성값 저장.(한 줄씩)

            // 데이터의 첫번째 줄은 column 명이다.
            // 이를 제외하고 데이터를 받기 위해 LenData 변수를 선언한다.
            LenData = Si_new.Length - 1;
            double[] wavelength_Si  = new double[LenData];
            double[] n_Si           = new double[LenData];
            double[] k_Si           = new double[LenData];

            // Si_new 에 받은 데이터를 각 컬럼별로 저장한다.
            LenData = Si_new.Length;
            for (int i = StartIndex; i < LenData; i++)
            {
                // tsv 형식의 데이터를 SingleLineData에 저장한다.
                SingleLineData = Si_new[i].Split((char)0x09);  // 0x09 : 수평 탭.

                // 각 컬럼에 해당하는 데이터를 저장한다.
                wavelength_Si[i - 1]    = Double.Parse(SingleLineData[0]);
                n_Si[i - 1]             = Double.Parse(SingleLineData[1]);
                k_Si[i - 1]             = Double.Parse(SingleLineData[2]);
            }


            // "SiO2_new.txt" 파일 읽기.
            string[] SiO2_new = File.ReadAllLines("SiO2_new.txt");  // Si 기판 물성값 저장.(한 줄씩)

            // 데이터의 첫번째 줄은 column 명이다.
            // 이를 제외하고 데이터를 받기 위해 LenData 변수를 선언한다.
            LenData = SiO2_new.Length - 1;
            double[] wavelength_SiO2    = new double[LenData];
            double[] n_SiO2             = new double[LenData];
            double[] k_SiO2             = new double[LenData];

            // SiO2_new 에 받은 데이터를 각 컬럼별로 저장한다.
            LenData = SiO2_new.Length;
            for (int i = StartIndex; i < LenData; i++)
            {
                // tsv 형식의 데이터를 SingleLineData에 저장한다.
                SingleLineData = SiO2_new[i].Split((char)0x09);  // 0x09 : 수평 탭.

                // 각 컬럼에 해당하는 데이터를 저장한다.
                wavelength_SiO2[i - 1]  = Double.Parse(SingleLineData[0]);
                n_SiO2[i - 1]           = Double.Parse(SingleLineData[1]);
                k_SiO2[i - 1]           = Double.Parse(SingleLineData[2]);
            }

            #region Si_new, SiO2_new 데이터 출력 (Test)
            /*LenData = wavelength_Si.Length;
            for (int i = 0; i < LenData; i++)
                WriteLine(wavelength_Si[i] + "\t" + n_Si[i] + "\t" + k_Si[i]);
            WriteLine("============================================");
            for (int i = 0; i < LenData; i++)
                WriteLine(wavelength_SiO2[i] + "\t" + n_SiO2[i] + "\t" + k_SiO2[i]);*/
            #endregion
            #endregion

            //
            // "Si_new.txt", "SiO2_new.txt" 의 n, k 를 사용하여
            // 파장에 따른 각 계면에서의 반사, 투과계수를 계산한다.
            //
            // 2021.03.24 이지원.
            //
            #region 각 계면에서의 반사, 투과계수 계산

            LenData = wavelength_Si.Length;

            // 반사계수를 담을 배열.
            Complex[] r01p = new Complex[LenData],
                      r01s = new Complex[LenData],
                      r12p = new Complex[LenData],
                      r12s = new Complex[LenData];

            // 투과계수를 담을 배열.
            Complex[] t01p = new Complex[LenData],
                      t01s = new Complex[LenData],
                      t12p = new Complex[LenData],
                      t12s = new Complex[LenData];

            double AOI_air  = degree2radian(65.0);  // air, SiO2 경계면에서의 입사각. (라디안) 
            Complex N_air   = new Complex(1, 0);    // 공기의 굴절률.

            // 반사, 투과계수를 계산한다.
            for (int i = 0; i < LenData; i++)
            {
                // 파장에 대한 물질의 복소굴절률을 구한다.
                Complex N_SiO2  = new Complex(n_SiO2[i], -k_SiO2[i]);
                Complex N_Si    = new Complex(n_Si[i], -k_Si[i]);

                // air, SiO2 경계면에서의 굴절각을 구한다. (스넬의 법칙)
                Complex Sintheta_j  = new Complex(Sin((double)AOI_air), 0);
                Complex Costheta_j  = new Complex(Cos((double)AOI_air), 0);
                Complex Sintheta_k  = (N_air / N_SiO2) * Sintheta_j;
                Complex theta_k     = Complex.Asin(Sintheta_k);
                // air, SiO2 경계면에서의 굴절각.
                Complex Costheta_k  = Complex.Cos(theta_k);

                // air, SiO2 경계면에서의 반사계수를 구한다.
                r01p[i] = ((N_SiO2 * Costheta_j) - (N_air * Costheta_k)) /
                               ((N_SiO2 * Costheta_j) + (N_air * Costheta_k));

                r01s[i] = ((N_air * Costheta_j) - (N_SiO2 * Costheta_k)) /
                               ((N_air * Costheta_j) + (N_SiO2 * Costheta_k));

                // air, SiO2 경계면에서의 투과계수를 구한다.
                t01p[i] = (N_air * Costheta_j * 2) /
                               ((N_SiO2 * Costheta_j) + (N_air * Costheta_k));

                t01s[i] = (N_air * Costheta_j * 2) /
                               ((N_air * Costheta_j) + (N_SiO2 * Costheta_k));

                // SiO2, Si 경계면에서의 굴절각을 구한다. (스넬의 법칙)
                Sintheta_j  = Complex.Sin(theta_k);
                Costheta_j  = Complex.Cos(theta_k);
                Sintheta_k  = (N_SiO2 / N_Si) * Sintheta_j;
                theta_k     = Complex.Asin(Sintheta_k);
                Costheta_k  = Complex.Cos(theta_k);

                // SiO2, Si 경계면에서의 반사계수를 구한다.
                r12p[i] = ((N_Si * Costheta_j) - (N_SiO2 * Costheta_k)) /
                             ((N_Si * Costheta_j) + (N_SiO2 * Costheta_k));

                r12s[i] = ((N_SiO2 * Costheta_j) - (N_Si * Costheta_k)) /
                             ((N_SiO2 * Costheta_j) + (N_Si * Costheta_k));

                // SiO2, Si 경계면에서의 투과계수를 구한다.
                t12p[i] = (N_SiO2 * Costheta_j * 2) /
                             ((N_Si * Costheta_j) + (N_SiO2 * Costheta_k));

                t12s[i] = (N_SiO2 * Costheta_j * 2) /
                             ((N_SiO2 * Costheta_j) + (N_Si * Costheta_k));
            }

            #region 위에서 구한 반사, 투과계수 출력 (Test)
            /*WriteLine("====== air, SiO2 경계 ======");
            for (int i = 0; i < LenData; i++)
            {
                WriteLine(
                    r01p[i] + " " +
                    r01s[i] + " " +
                    t01p[i] + " " +
                    t01s[i]);
            }
            WriteLine("====== SiO2, Si 경계 ======");
            for (int i = 0; i < LenData; i++)
            {
                WriteLine(
                    r12p[i] + " " +
                    r12s[i] + " " +
                    t12p[i] + " " +
                    t12s[i]);
            }*/
            #endregion

            #endregion

            //
            // 위상 두께를 구하고 위에서 구한 반사계수를 통해
            // 무한등비급수 수렴식을 계산한다. => 총 반사계수를 구한다.
            //
            // 2021.03.24 이지원. 
            //
            #region 무한등비급수 수렴식 계산

            // 총 반사계수를 저장할 배열 선언.
            Complex[] Rp = new Complex[LenData],
                      Rs = new Complex[LenData];

            for (int i = 0; i < LenData; i++)
            {
                // SiO2의 복소 굴절률.
                Complex N_SiO2 = new Complex(n_SiO2[i], -k_SiO2[i]);

                // air, SiO2 경계면에서의 굴절각을 구한다. (스넬의 법칙)
                Complex Sintheta_j  = new Complex(Sin((double)AOI_air), 0);
                Complex Sintheta_k  = (N_air / N_SiO2) * Sintheta_j;
                Complex theta_k     = Complex.Asin(Sintheta_k);
                // air, SiO2 경계면에서의 굴절각.
                Complex Costheta_k  = Complex.Cos(theta_k);

                // 위상 두께를 구한다.
                Complex PhaseThickness = (1000 * Math.PI * 2) * N_SiO2 * Costheta_k /
                                          wavelength_SiO2[i];
                //WriteLine(PhaseThickness);

                // 총 반사계수를 구한다.
                Complex E = Complex.Exp(PhaseThickness * new Complex(0, -2.0));

                // 무한등비급수 공식 1.
                Rp[i] = (r01p[i] + r12p[i] * E) /
                        (1 + r01p[i] * r12p[i] * E);

                Rs[i] = (r01s[i] + r12s[i] * E) /
                        (1 + r01s[i] * r12s[i] * E);
            }

            #region 총 반사계수 출력 (Test)

            /*for (int i = 0; i < LenData; i++)
                //WriteLine(Pow(Rp[i].Magnitude, 2));
                WriteLine(Pow(Rs[i].Magnitude, 2));*/

            #endregion

            #endregion

            //
            // 위에서 구한 총 반사계수로부터 alpha, beta 를 구한다.
            //
            // 2021.03.24 이지원.
            //
            #region 총 반사계수로부터 alpha, beta 도출.

            // alpha, beta 이론값을 담을 배열 선언.
            double[] alpha_cal  = new double[LenData],
                     beta_cal   = new double[LenData];

            // Polarizer 오프셋 각.
            double polarizerAngle = degree2radian(45.0);

            for (int i = 0; i < LenData; i++)
            {
                // 총 반사계수비. (복소반사계수비)
                Complex rho = Rp[i] / Rs[i];

                // Psi, Delta.
                double Psi      = Atan(rho.Magnitude);
                double Delta    = rho.Phase;
                
                // double Radian2Degree(double angle) => (angle * (180.0 / PI));
                // WriteLine(Radian2Degree(Psi));
                // WriteLine(Radian2Degree(Delta));

                alpha_cal[i] = (Pow(Tan(Psi), 2) - Pow(Tan(polarizerAngle), 2)) /
                               (Pow(Tan(Psi), 2) + Pow(Tan(polarizerAngle), 2));

                beta_cal[i]  = (2 * Tan(Psi) * Cos(Delta) * Tan(polarizerAngle)) /
                               (Pow(Tan(Psi), 2) + Pow(Tan(polarizerAngle), 2));

                // WriteLine(alpha_cal[i]);
                // WriteLine(beta_cal[i]);
            }

            #region alpha, beta 이론값 출력 (Test)

            /*for (int i = 0; i < LenData; i++)
                WriteLine(alpha_cal[i] + " " 
                + beta_cal[i]);*/

            #endregion

            #endregion

            //
            // 측정값과 이론값의 MSE 를 계산한다.
            //
            // 2021.03.24 이지원.
            //
            #region 측정값과 이론값의 MSE 계산.

            double difference_MSE = 0;
            double sum = 0;
            for (int i = 0; i < LenData; i++)
            {
                difference_MSE = 
                    Pow((alpha_exp[i] - alpha_cal[i]), 2) + 
                    Pow((beta_exp[i] - beta_cal[i]), 2);
                sum += difference_MSE;
                //WriteLine(difference_MSE);
            }

            double MSE = sum / LenData;
            WriteLine($"MSE : {MSE}");
            #endregion
        }
    }
}
