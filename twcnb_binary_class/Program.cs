using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Iveonik.Stemmers;

namespace twcnb_binary_class
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> token = new List<string>();
            List<char> label = new List<char>();
            List<char> drug_name = new List<char>();
            Dictionary<int, string> tweets = new Dictionary<int, string>();
            Dictionary<int, string> term = new Dictionary<int, string>();
            Dictionary<string, decimal> ifl = new Dictionary<string, decimal>();
            Dictionary<string, decimal> icc = new Dictionary<string, decimal>();
            Dictionary<string, decimal> cpn = new Dictionary<string, decimal>();
            Dictionary<string, decimal> cpp = new Dictionary<string, decimal>();
            Dictionary<string, decimal> dpn = new Dictionary<string, decimal>();
            Dictionary<string, decimal> dpp = new Dictionary<string, decimal>();
            Dictionary<string, decimal> fwn = new Dictionary<string, decimal>();
            Dictionary<string, decimal> fwp = new Dictionary<string, decimal>();
            Dictionary<string, decimal> fwl2n = new Dictionary<string, decimal>();
            Dictionary<string, decimal> fwl2p = new Dictionary<string, decimal>();
            StreamReader sr = new StreamReader(@"AMIA_task1_training+dev_hun+pos+dsg+drug.arff");
            //StreamWriter sw = new StreamWriter(@"AMIA_task1_training+dev_hun+pos+dsg+drug+stemmer+lowercase+stopwords+twcnb.arff");
            //StreamWriter sw_n = new StreamWriter(@"TWCNB_testset_fwl2_n.txt");
            //StreamWriter sw_p = new StreamWriter(@"TWCNB_testset_fwl2_p.txt");
            EnglishStemmer es = new EnglishStemmer();
            string str = sr.ReadLine();
            int count = 0;
            int index = 0;
            int label_0_count = 0;
            int label_1_count = 0;
            double dn = 0;
            double dp = 0;
            decimal fwn_l2 = 0;
            decimal fwp_l2 = 0;
            while (str != "@data")
            {
                str = sr.ReadLine();
            }
            while (str != null)
            {
                str = sr.ReadLine();
                if (str == null)
                {
                    break;
                }
                label.Add(str[str.Count() - 3]); // label
                drug_name.Add(str[str.Count() - 1]); // drug name*/
                str = str.Remove(str.Count() - 5, 5);
                str = str.Remove(0, 1);
                str = es.Stem(str);
                str = str.ToLower();
                tweets.Add(index, str);
                index++;
                string[] term_split;
                string[] reg;
                str = str.Replace("and", ",");
                str = str.Replace("or", ",");
                str = str.Replace(";", ",");
                str = str.Replace(":", ",");
                str = str.Replace("?", ",");
                str = str.Replace("!", ",");
                str = str.Replace(".", ",");
                term_split = str.Split(',');
                str = str.Replace(",", "");
                reg = str.Split(' ');
                #region tokenize
                foreach (var item in reg)
                {
                    if (!token.Contains(item))
                    {
                        if (!item.Equals(""))
                        {
                            if(!item.Contains("%"))
                            {
                                if (!item.Contains("{"))
                                {
                                    if (!item.Contains("}"))
                                    {
                                        if (!item.Contains(@"\\"))
                                        {
                                            string stopword;
                                            int nsc = 0;
                                            StreamReader stopwords = new StreamReader(@"stopwords.txt");
                                            stopword = stopwords.ReadLine();
                                            while (stopword != null)
                                            {
                                                if (item == stopword)
                                                {
                                                    nsc++;
                                                }
                                                stopword = stopwords.ReadLine();
                                            }
                                            if (nsc == 0)
                                            {
                                                token.Add(item);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        continue;
                    }
                }
                #endregion
                #region term
                foreach (var item in term_split)
                {
                    if (item == " ")
                    {
                        continue;
                    }
                    term.Add(count, item);
                    count++;
                }
            }
            #endregion
            #region Label Count
            foreach (var item in label)
            {
                if (item.Equals('0'))
                {
                    label_0_count++;
                }
                else
                {
                    label_1_count++;
                }
            }
            #endregion
            #region Inverse average fragment length
            foreach (var item in token)
            {
                int label_0 = 0;
                int label_1 = 0;
                int ft = 0;
                int lf = 0;
                string[] split;
                for (int i = 0; i < term.Count; i++)
                {
                    int watch = 0;
                    split = term[i].Split(' ');
                    for (int j = 0; j < split.Length; j++)
                    {
                        if (split[j] == item)
                        {
                            lf += split.Length;
                            watch++;
                        }
                    }
                    if (watch > 0)
                    {
                        ft++;
                    }
                }
                if (ft == 0 || lf == 0)
                {
                    ifl.Add(item, 0);
                }
                else
                {
                    ifl.Add(item, (decimal)ft / lf);
                }
                #endregion
                #region Inverse category count
                for (int i = 0; i < tweets.Count; i++)
                {
                    split = tweets[i].Split(' ');
                    for (int j = 0; j < split.Length; j++)
                    {
                        if (split[j] == item)
                        {
                            if (label[i] == '0')
                            {
                                label_0++;
                            }
                            else
                            {
                                label_1++;
                            }
                        }
                    }
                }
                if (label_0 > 0 && label_1 > 0)
                {
                    icc.Add(item, (decimal)0.5);
                }
                else
                {
                    icc.Add(item, 1);
                }
                #endregion
                #region Category probability
                #region Negative
                if (label_0 == 0 || label_1 == 0)
                {
                    cpn.Add(item, 0);
                }
                else
                {
                    cpn.Add(item, (decimal)label_0 / (label_0 + label_1));
                }
                #region Positive
                if (label_0 == 0 || label_1 == 0)
                {
                    cpp.Add(item, 0);
                }
                else
                {
                    cpp.Add(item, (decimal)label_1 / (label_0 + label_1));
                }
                #endregion
                #endregion
                #endregion
                #region Document probability
                #region Negative
                dpn.Add(item, (decimal)label_0 / label_0_count);
                #region Positive
                dpp.Add(item, (decimal)label_1 / label_1_count);
                #endregion
                #endregion
                #endregion
                #region Negative
                fwn.Add(item, (ifl[item] + icc[item]) * (cpn[item] + dpn[item]));
                fwn_l2 += ((ifl[item] + icc[item]) * (cpn[item] + dpn[item])) * ((ifl[item] + icc[item]) * (cpn[item] + dpn[item]));
                #endregion
                #region Positive
                fwp.Add(item, (ifl[item] + icc[item]) * (cpp[item] + dpp[item]));
                fwp_l2 += ((ifl[item] + icc[item]) * (cpp[item] + dpp[item])) * ((ifl[item] + icc[item]) * (cpp[item] + dpp[item]));
                #endregion
            }
            dn = (double)fwn_l2;
            dn = Math.Sqrt(dn);
            dp = (double)fwp_l2;
            dp = Math.Sqrt(dp);
            fwn_l2 = (decimal)dn;
            fwp_l2 = (decimal)dp;
            /*sw.WriteLine("@relation AMIA_task1_training+dev_hun+pos+dsg+drug+twcnb");
            sw.WriteLine();
            sw.WriteLine("@attribute Label {0,1}");
            sw.WriteLine("@attribute Drug_name numeric");*/
            foreach (var item in token)
            {
                /*int exception = 0;
                for (int i = 0; i < item.Count(); i++)
                {
                    if (item[i] == '\'')
                    {
                        exception++;
                    }
                    if (item[i] == '\"')
                    {
                        exception++;
                    }
                }
                if (exception > 0)
                {
                    sw.WriteLine("@attribute \"" + item + "_negative\"" + " numeric");
                    sw.WriteLine("@attribute \"" + item + "_positive\"" + " numeric");
                }
                else
                {
                    sw.WriteLine("@attribute " + item + "_negative" + " numeric");
                    sw.WriteLine("@attribute " + item + "_positive" + " numeric");
                }*/
                fwl2n.Add(item, fwn[item] / fwn_l2);
                fwl2p.Add(item, fwp[item] / fwp_l2);
            }
            /*var dicSort_n = from objDic in fwl2n orderby objDic.Value descending select objDic;
            var dicSort_p = from objDic in fwl2p orderby objDic.Value descending select objDic;
            int stop = 0;
            foreach (KeyValuePair<string, decimal> kvp in dicSort_n)
            {
                if (stop < 30)
                {
                    sw_n.WriteLine(kvp.Key + " " + kvp.Value);
                    stop++;
                }
                else
                {
                    break;
                }
            }
            stop = 0;
            foreach (KeyValuePair<string, decimal> kvp in dicSort_p)
            {
                if (stop < 30)
                {
                    sw_p.WriteLine(kvp.Key + " " + kvp.Value);
                    stop++;
                }
                else
                {
                    break;
                }
            }
            sw.WriteLine("@attribute Tweet string"); //tweets
            sw.WriteLine();
            sw.WriteLine("@data");
            string[] tokens;
            for (int i = 0; i < tweets.Count; i++)
            {
                tokens = tweets[i].Split(' ');
                if (label[i] == '1')
                {
                    sw.Write("{0 1");
                    if (drug_name[i] == '1')
                    {
                        Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
                        sw.Write(", 1 1");
                        for (int j = 0; j < tokens.Count(); j++)
                        {
                            int n_count = 2;
                            int p_count = 3;
                            for (int k = 0; k < token.Count; k++)
                            {
                                if (tokens[j] == token[k])
                                {
                                    if (fwl2n[token[k]] > (decimal)0.025) //!= 0
                                    {
                                        if (!dic.ContainsKey(n_count))
                                        {
                                            dic.Add(n_count, fwl2n[token[k]]);
                                        }
                                    }
                                    if(fwl2p[token[k]] > (decimal)0.025) //!= 0
                                    {
                                        if(!dic.ContainsKey(p_count))
                                        {
                                            dic.Add(p_count, fwl2p[token[k]]);
                                        }
                                    }
                                    break;
                                }
                                n_count += 2;
                                p_count += 2;
                            }
                        }
                        var dicSort = from objDic in dic orderby objDic.Key ascending select objDic;
                        foreach (KeyValuePair<int, decimal> kvp in dicSort)
                        {
                            sw.Write(", " + kvp.Key + " " + kvp.Value);
                        }
                        sw.Write(", 29392 " + "\""+tweets[i]+"\"");
                        sw.WriteLine("}");
                        continue;
                    }
                    else
                    {
                        Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
                        for (int j = 0; j < tokens.Count(); j++)
                        {
                            int n_count = 2;
                            int p_count = 3;
                            for (int k = 0; k < token.Count; k++)
                            {
                                if (tokens[j] == token[k])
                                {
                                    if (fwl2n[token[k]] > (decimal)0.025) //!= 0
                                    {
                                        if (!dic.ContainsKey(n_count))
                                        {
                                            dic.Add(n_count, fwl2n[token[k]]);
                                        }
                                    }
                                    if(fwl2p[token[k]] > (decimal)0.025) //!= 0
                                    {
                                        if(!dic.ContainsKey(p_count))
                                        {
                                            dic.Add(p_count, fwl2p[token[k]]);
                                        }
                                    }
                                    break;
                                }
                                n_count += 2;
                                p_count += 2;
                            }
                        }
                        var dicSort = from objDic in dic orderby objDic.Key ascending select objDic;
                        foreach (KeyValuePair<int, decimal> kvp in dicSort)
                        {
                            sw.Write(", " + kvp.Key + " " + kvp.Value);
                        }
                        sw.Write(", 29392 " + "\""+tweets[i]+"\"");
                        sw.WriteLine("}");
                        continue;
                    }
                }
                else
                {
                    sw.Write("{0 0");
                    if (drug_name[i] == '1')
                    {
                        Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
                        sw.Write(", 1 1");
                        for (int j = 0; j < tokens.Count(); j++)
                        {
                            int n_count = 2;
                            int p_count = 3;
                            for (int k = 0; k < token.Count; k++)
                            {
                                if (tokens[j] == token[k])
                                {
                                    if (fwl2n[token[k]] > (decimal)0.025) //!= 0
                                    {
                                        if (!dic.ContainsKey(n_count))
                                        {
                                            dic.Add(n_count, fwl2n[token[k]]);
                                        }
                                    }
                                    if(fwl2p[token[k]] > (decimal)0.025) //!= 0
                                    {
                                        if (!dic.ContainsKey(p_count))
                                        {
                                            dic.Add(p_count, fwl2p[token[k]]);
                                        }
                                    }
                                    break;
                                }
                                n_count += 2;
                                p_count += 2;
                            }
                        }
                        var dicSort = from objDic in dic orderby objDic.Key ascending select objDic;
                        foreach (KeyValuePair<int, decimal> kvp in dicSort)
                        {
                            sw.Write(", " + kvp.Key + " " + kvp.Value);
                        }
                        sw.Write(", 29392 " +"\""+tweets[i]+"\"");
                        sw.WriteLine("}");
                        continue;
                    }
                    else
                    {
                        Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
                        for (int j = 0; j < tokens.Count(); j++)
                        {
                            int n_count = 2;
                            int p_count = 3;
                            for (int k = 0; k < token.Count; k++)
                            {
                                if (tokens[j] == token[k])
                                {
                                    if (fwl2n[token[k]] > (decimal)0.025) //!= 0
                                    {
                                        if (!dic.ContainsKey(n_count))
                                        {
                                            dic.Add(n_count, fwl2n[token[k]]);
                                        }
                                    }
                                    if(fwl2p[token[k]] > (decimal)0.025) //!= 0
                                    {
                                        if (!dic.ContainsKey(p_count))
                                        {
                                            dic.Add(p_count, fwl2p[token[k]]);
                                        }
                                    }
                                    break;
                                }
                                n_count += 2;
                                p_count += 2;
                            }
                        }
                        var dicSort = from objDic in dic orderby objDic.Key ascending select objDic;
                        foreach (KeyValuePair<int, decimal> kvp in dicSort)
                        {
                            sw.Write(", " + kvp.Key + " " + kvp.Value);
                        }
                        sw.Write(", 29392 " + "\""+tweets[i]+"\"");
                        sw.WriteLine("}");
                        continue;
                    }
                }
            }*/

            sr.Close();
            /*sw.Close();
            sw_n.Close();
            sw_p.Close();*/
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}