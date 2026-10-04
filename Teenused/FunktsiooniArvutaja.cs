using Math_Functions.Models;
using NCalc;
using System.Globalization;

namespace Math_Functions.Teenused
{
    public class FunktsiooniArvutaja
    {
        private const double AlgusX = -10;
        private const double LoppX = 10;
        private const double Samm = 0.01;

        public const string NumbrilineTekst = "numbriline (keskmine vahe)";

        public static bool TuletisOnTuhi(string? tuletis)
        {
            return string.IsNullOrWhiteSpace(tuletis) || tuletis.StartsWith("numbriline");
        }

        public double? ArvutaY(string valem, double x)
        {
            try
            {
                var avaldis = new Expression(valem.Replace("^", "**"));
                avaldis.Parameters["x"] = x;

                double tulemus = Convert.ToDouble(avaldis.Evaluate(), CultureInfo.InvariantCulture);

                if (double.IsNaN(tulemus) || double.IsInfinity(tulemus))
                {
                    return null;
                }
                return tulemus;
            }
            catch
            {
                return null;
            }
        }

        public double? ArvutaVaartus(FunktsiooniUurimine olem, bool kasTuletis, double x)
        {
            if (!kasTuletis)
            {
                return ArvutaY(olem.Valem, x);
            }

            if (!TuletisOnTuhi(olem.Tuletis))
            {
                return ArvutaY(olem.Tuletis, x);
            }
            double h = 0.00001;
            double? a = ArvutaY(olem.Valem, x + h);
            double? b = ArvutaY(olem.Valem, x - h);
            if (a == null || b == null)
            {
                return null;
            }
            return (a.Value - b.Value) / (2 * h);
        }

        public bool ValemOnKorras(string valem)
        {
            if (string.IsNullOrWhiteSpace(valem))
            {
                return false;
            }

            double[] katsePunktid = { -3.7, -1.3, 0.5, 1.7, 2.9, 5.3 };
            foreach (double x in katsePunktid)
            {
                if (ArvutaY(valem, x) != null)
                {
                    return true;
                }
            }
            return false;
        }

        public void Analuusi(FunktsiooniUurimine olem)
        {
            olem.Valem = olem.Valem.Trim();

            if (TuletisOnTuhi(olem.Tuletis))
            {
                olem.Tuletis = NumbrilineTekst;
            }
            else
            {
                olem.Tuletis = olem.Tuletis.Trim();
            }

            olem.Maaramispiirkond = LeiaMaaramispiirkond(olem);

            List<double> nullkohad = LeiaNullkohad(olem, false);
            List<double> kriitilised = LeiaNullkohad(olem, true);

            olem.Nullkohad = KirjutaNimekiri(nullkohad);
            olem.KriitilisedPunktid = KirjutaNimekiri(kriitilised);

            var ekstreemumid = new List<string>();
            foreach (double x in kriitilised)
            {
                double? vasak = ArvutaVaartus(olem, true, x - 0.01);
                double? parem = ArvutaVaartus(olem, true, x + 0.01);
                double? y = ArvutaVaartus(olem, false, x);

                if (vasak == null || parem == null || y == null)
                {
                    continue;
                }

                if (vasak > 0 && parem < 0)
                {
                    ekstreemumid.Add($"max (x = {Ume(x)}, y = {Ume(y.Value)})");
                }
                else if (vasak < 0 && parem > 0)
                {
                    ekstreemumid.Add($"min (x = {Ume(x)}, y = {Ume(y.Value)})");
                }
            }

            olem.Ekstreemumid = ekstreemumid.Count == 0
                ? "puuduvad lõigul [-10; 10]"
                : string.Join("; ", ekstreemumid);
        }

        private string LeiaMaaramispiirkond(FunktsiooniUurimine olem)
        {
            var augud = new List<double>();
            int punkte = (int)Math.Round((LoppX - AlgusX) / Samm);

            for (int i = 0; i <= punkte; i++)
            {
                double x = Math.Round(AlgusX + i * Samm, 6);
                if (ArvutaY(olem.Valem, x) == null)
                {
                    augud.Add(x);
                }
            }

            if (augud.Count == 0)
            {
                return "X: (-∞; ∞)";
            }

            var osad = new List<string>();
            double algus = augud[0];
            double eelmine = augud[0];

            for (int i = 1; i < augud.Count; i++)
            {
                if (augud[i] - eelmine > Samm * 1.5)
                {
                    osad.Add(KirjutaLoik(algus, eelmine));
                    algus = augud[i];
                }
                eelmine = augud[i];
            }
            osad.Add(KirjutaLoik(algus, eelmine));

            return "X: R, välja arvatud " + string.Join(", ", osad) + " (lõigul [-10; 10])";
        }

        private string KirjutaLoik(double algus, double lopp)
        {
            if (lopp - algus < 0.05)
            {
                return "x = " + Ume((algus + lopp) / 2);
            }
            return "x ∈ [" + Ume(algus) + "; " + Ume(lopp) + "]";
        }

        private List<double> LeiaNullkohad(FunktsiooniUurimine olem, bool kasTuletis)
        {
            var tulemus = new List<double>();
            double? eelmineY = null;
            double eelmineX = 0;
            int punkte = (int)Math.Round((LoppX - AlgusX) / Samm);

            for (int i = 0; i <= punkte; i++)
            {
                double x = Math.Round(AlgusX + i * Samm, 6);
                double? y = ArvutaVaartus(olem, kasTuletis, x);

                if (y != null)
                {
                    if (Math.Abs(y.Value) < 0.000000001)
                    {
                        LisaKuiUus(tulemus, x);
                    }
                    else if (eelmineY != null && eelmineY.Value * y.Value < 0)
                    {
                        double? nullkoht = Poolita(olem, kasTuletis, eelmineX, x);
                        if (nullkoht != null)
                        {
                            LisaKuiUus(tulemus, nullkoht.Value);
                        }
                    }
                }

                eelmineY = y;
                eelmineX = x;
            }

            return tulemus;
        }

        private double? Poolita(FunktsiooniUurimine olem, bool kasTuletis, double a, double b)
        {
            double? fa = ArvutaVaartus(olem, kasTuletis, a);
            if (fa == null)
            {
                return null;
            }

            for (int i = 0; i < 60; i++)
            {
                double keskel = (a + b) / 2;
                double? fk = ArvutaVaartus(olem, kasTuletis, keskel);
                if (fk == null)
                {
                    return null;
                }

                if (fa.Value * fk.Value <= 0)
                {
                    b = keskel;
                }
                else
                {
                    a = keskel;
                    fa = fk;
                }
            }

            double x = (a + b) / 2;
            double? y = ArvutaVaartus(olem, kasTuletis, x);

            if (y != null && Math.Abs(y.Value) < 0.0001)
            {
                return x;
            }
            return null;
        }

        private void LisaKuiUus(List<double> nimekiri, double x)
        {
            foreach (double olemasolev in nimekiri)
            {
                if (Math.Abs(olemasolev - x) < 0.001)
                {
                    return;
                }
            }
            nimekiri.Add(x);
        }

        private string KirjutaNimekiri(List<double> nimekiri)
        {
            if (nimekiri.Count == 0)
            {
                return "puuduvad lõigul [-10; 10]";
            }

            var tekstid = new List<string>();
            for (int i = 0; i < nimekiri.Count; i++)
            {
                tekstid.Add($"x{i + 1} = {Ume(nimekiri[i])}");
            }
            return string.Join(", ", tekstid);
        }

        private string Ume(double arv)
        {
            arv = Math.Round(arv, 4);
            if (arv == 0)
            {
                arv = 0;
            }
            return arv.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
