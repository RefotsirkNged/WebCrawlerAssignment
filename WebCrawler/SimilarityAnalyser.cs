using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    class SimilarityAnalyser
    {
        private List<string> salts;
        private List<List<int>> allSuperShingles;

        public SimilarityAnalyser()
        {
            salts = new List<string>()
            {
                "kwckz87s",
                "9lcm3umf",
                "6yqg2k92",
                "bxms5djy",
                "ajxvjwe1",
                "2jte9vhk",
                "5nspggci",
                "lki749lt",
                "4k93uyti",
                "bs6t36kb",
                "pr1g2b65",
                "3mjryd80",
                "szr03twt",
                "9e9t88g5",
                "d1vmgxds",
                "qnrn35v7",
                "0jmv3apy",
                "v7wepvyj",
                "3wmwoykx",
                "w9taincd",
                "36lfq1vi",
                "q3gh5k0u",
                "q0g1bh2g",
                "oiz51ln6",
                "uk9xfdis",
                "evv1dnms",
                "jyd4wtmg",
                "v415596c",
                "6fgn4xba",
                "cpdwyh7z",
                "r13tdbne",
                "scm3ah2g",
                "m9zr139k",
                "jy2z5agi",
                "v2oo0tul",
                "bq3jnp6t",
                "c6hrt2m9",
                "6lfdcn7t",
                "4thiqrjv",
                "6dodf2k9",
                "55qepefi",
                "vkp3g93g",
                "r9e1tdye",
                "ejb4dd4j",
                "plmfb19l",
                "ynkkxoph",
                "hlxygg7p",
                "9o0ppzsx",
                "cy3zydvb",
                "1wy5a81i",
                "101hoqb9",
                "x30l0jwc",
                "beu1dfc6",
                "3u5jeln3",
                "ksgp5be7",
                "v9xlxes3",
                "agdwcl23",
                "bjf7y7yj",
                "96jb7ymm",
                "r2j7yq0m",
                "1ehp4wnx",
                "yzekqp12",
                "rl553qjg",
                "f3selqs6",
                "d02qwsph",
                "r7oyqphg",
                "5st8wywi",
                "dg5m27i0",
                "1nzhsrkd",
                "iruvr0w3",
                "k63cnypv",
                "10vpirbi",
                "emsyoh27",
                "o8x3sut9",
                "wv9su92n",
                "aqdn90uo",
                "vbls8pqf",
                "p7qw93rp",
                "ggn27k8j",
                "pbr17zjt",
                "udkmrekn",
                "3w2d6ivx",
                "mn88h0mx",
                "ih8x20tm"
            };

            allSuperShingles = new List<List<int>>();
        }

        //do not worry, not worry about, worry about your, about your difficulties, your
        //difficulties in, difficulties in mathematics
        private List<string> ShingleText(string Text)
        {
            Text = Text.Replace(",", "");
            Text = Text.Replace(".", "");
            List<string> ShingledText = new List<string>();
            List<string> IndividualWords = Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList();

            for (int i = 0; i < IndividualWords.Count() - 2; i++)
            {
                ShingledText.Add(IndividualWords[i] + " " + IndividualWords[i + 1] + " " + IndividualWords[i + 2]);
            }
            return ShingledText;
        }

        private double JaccardVanilla(List<string> ShingledFirstText, List<string> ShingledSecondText)
        {
            double jaccardSimil = -1;

            var overlap = ShingledFirstText.Intersect(ShingledSecondText).ToList();
            var union = ShingledFirstText.Union(ShingledSecondText).ToList();

            jaccardSimil = (double)overlap.Count() / (double)union.Count();
            return jaccardSimil;
        }


        private double JaccardUsingMultipleHashes(List<string> ShingledFirstText, List<string> ShingledSecondText)
        {
            double jaccardSimil = -1;

            return jaccardSimil;

        }


        private double JaccardUsingRandomisedPermutations(List<string> ShingledFirstText, List<string> ShingledSecondText)
        {
            double jaccardSimil = -1;



            return jaccardSimil;
        }


        public List<double> JaccardSimilarityAnalysis(string FirstText, string SecondText)
        {
            List<double> results = new List<double>();

            List<string> ShingledFirstText = ShingleText(FirstText);
            List<string> ShingledSecondText = ShingleText(SecondText);

            results.Add(JaccardVanilla(ShingledFirstText, ShingledSecondText));
            results.Add(JaccardUsingMultipleHashes(ShingledFirstText, ShingledSecondText));
            results.Add(JaccardUsingRandomisedPermutations(ShingledFirstText, ShingledSecondText));

            return results;
        }

        public double PerformVanillaAnalysis(string FirstText, string SecondText)
        {
            double result = 0;
            List<string> ShingledFirstText = ShingleText(FirstText);
            List<string> ShingledSecondText = ShingleText(SecondText);
            result = JaccardVanilla(ShingledFirstText, ShingledSecondText);
            return result;
        }

        public bool IsTextDuplicate(string text)
        {
            List<int> superShingles = GetSuperShingles(text);


            foreach (List<int> existingSuperShingle in allSuperShingles)
            {
                int matches = 0;
                foreach (int currentSuperShignle in superShingles)
                {
                    foreach (int shingleToCompare in existingSuperShingle)
                    {
                        if (currentSuperShignle == shingleToCompare)
                        {
                            if (++matches == 2)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            allSuperShingles.Add(superShingles);
            return false;
        }

        private List<int> GetSuperShingles(string text)
        {
            List<int> superShingles = new List<int>();
            List<string> shingles = ShingleText(text);
            List<int> fingerprints = new List<int>();

            foreach (string salt in salts)
            {
                List<int> tempFingerprints = new List<int>();

                foreach (string shingle in shingles)
                {
                    string salted = shingle + salt;
                    tempFingerprints.Add(salted.GetHashCode());
                }


                tempFingerprints.Sort();

                fingerprints.Add(tempFingerprints.First());
            }

            while (fingerprints.Count() > 0)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < 6; i++)
                {
                    sb.Append(fingerprints.First());
                    fingerprints.RemoveAt(0);
                }

                superShingles.Add(sb.ToString().GetHashCode());
            }

            return superShingles;
            //allSuperShingles.Add(superShingles);
        }
    }
}
