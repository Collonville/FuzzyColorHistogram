using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyColorHistogram1
{
    class DataSet
    {

        /// ---------------------------------------------------------------------------------------
        /// <summary>
        ///     指定した検索パターンに一致するファイルを最下層まで検索しすべて返します。</summary>
        /// <param name="stRootPath">
        ///     検索を開始する最上層のディレクトリへのパス。</param>
        /// <param name="stPattern">
        ///     パス内のファイル名と対応させる検索文字列。</param>
        /// <returns>
        ///     検索パターンに一致したすべてのファイルパス。</returns>
        /// ---------------------------------------------------------------------------------------
        private static string[] GetFilesMostDeep(string stRootPath, string stPattern)
        {
            System.Collections.Specialized.StringCollection hStringCollection = (
                new System.Collections.Specialized.StringCollection()
            );

            foreach (string stFilePath in System.IO.Directory.GetFiles(stRootPath, stPattern))
            {
                hStringCollection.Add(stFilePath);
            }

            foreach (string stDirPath in System.IO.Directory.GetDirectories(stRootPath))
            {
                string[] stFilePathes = GetFilesMostDeep(stDirPath, stPattern);

                if (stFilePathes != null)
                {
                    hStringCollection.AddRange(stFilePathes);
                }
            }

            string[] stReturns = new string[hStringCollection.Count];
            hStringCollection.CopyTo(stReturns, 0);

            return stReturns;
        }

        public static void openAllDataSet(ref List<string> dateSetPath)
        {
            // ファイル名に「Hoge」を含み、拡張子が「.txt」のファイルを最下層まで検索し取得する
            string[] stFilePathes = GetFilesMostDeep(@"C:\Users\ht235_000\Documents\Laboratory\ColorWheel\Dataset\microsoft\", "**.png");
            string stPrompt = string.Empty;

            // 取得したファイル名を列挙する
            foreach (string stFilePath in stFilePathes)
            {
                dateSetPath.Add(stFilePath);
            }

            Console.WriteLine("Dataset data opened");
        }
    }
}
