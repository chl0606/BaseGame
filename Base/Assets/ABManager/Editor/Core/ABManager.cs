using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using System.IO;
using LitJson;

namespace ABManager
{
    public class ABManager
    {
        public const string PATH_DATA = "Assets/ABManager";
        public const string VALUE_MANIFEST = "AssetBundleManifest";

        public static void SetAssetBundleName(string path, string assetBundleName, string assetBundleVariant)
        {
            AssetImporter ai = AssetImporter.GetAtPath(path);

            if (string.IsNullOrEmpty(assetBundleName))
            {
                ai.assetBundleVariant = assetBundleVariant;
                ai.assetBundleName = assetBundleName;
            }
            else
            {
                ai.assetBundleName = assetBundleName;
                ai.assetBundleVariant = assetBundleVariant;
            }
            
            //ai.SaveAndReimport();
        }

        public static List<string> GetAllAssetBundlesWithVariant()
        {
            string manifestPath = GetManifestPath();
            WWW www = new WWW(manifestPath);
            AssetBundle manifestBundle = www.assetBundle;

            if (null == manifestBundle)
            {
                BuildAllAssetBundles(false, false);
                www = new WWW(manifestPath);
                manifestBundle = www.assetBundle;
            }

            AssetBundleManifest manifest = (AssetBundleManifest)www.assetBundle.LoadAsset(VALUE_MANIFEST, typeof(AssetBundleManifest));

            List<string> variantList = new List<string>(manifest.GetAllAssetBundlesWithVariant());
            manifestBundle.Unload(true);
            www.Dispose();
            www = null;

            //foreach (string str in variantList)
            //{
            //    Debug.Log(str);
            //}

            return variantList;
        }

        public static void BuildAllAssetBundles(bool showExplorer, bool replace)
        {
            string outputPath = Path.Combine(ABUtil.PATH_OUTPUT, ABUtil.GetPlatformName());

            if (replace)
            {
                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, true);
            }

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            //Build AssetBundle
            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

            //Read Manifest
            string manifestPath = GetManifestPath();
            WWW www = new WWW(manifestPath);
            AssetBundle manifestBundle = www.assetBundle;
            AssetBundleManifest manifest = (AssetBundleManifest)www.assetBundle.LoadAsset(VALUE_MANIFEST, typeof(AssetBundleManifest));
            Debug.Log("[manifestPath] " + manifestPath);

            #region ABData.json
            string[] names = manifest.GetAllAssetBundles();
            List<string> variantList = new List<string>(manifest.GetAllAssetBundlesWithVariant());
            List<ABData> dataList = new List<ABData>();
            ABData data;
            char charDot = '.';
            for (int i = 0; i < names.Length; i++)
            {
                data = new ABData();

                //name
                data.name = names[i];

                //variant
                if (variantList.Contains(names[i]))
                {
                    string[] nameSplit = names[i].Split(charDot);
                    data.variant = nameSplit[nameSplit.Length - 1];
                }
                else
                {
                    data.variant = string.Empty;
                }

                //assets
                string[] paths = AssetDatabase.GetAssetPathsFromAssetBundle(names[i]);
                data.assets = new string[paths.Length];
                for (int j = 0; j < paths.Length; j++)
                {
                    string[] pathSplit = paths[j].Split('/');
                    pathSplit = pathSplit[pathSplit.Length - 1].Split('.');
                    data.assets[j] = pathSplit[0];
                }

                //file size
                FileInfo fi = new FileInfo(GetBundlePath(data.name));
                data.size = fi.Length.ToString();

                dataList.Add(data);
            }

            //Save json
           string jsonPath = PATH_DATA;
            if (!Directory.Exists(jsonPath))
                Directory.CreateDirectory(jsonPath);
            jsonPath = Path.Combine(jsonPath, ABUtil.KEY_JSONNAME + ".json");
            Debug.Log("[jsonPath] " + jsonPath);

            JsonWriter jw = new JsonWriter();
            jw.PrettyPrint = true;
            JsonMapper.ToJson(dataList, jw);
            string jsonStr = jw.ToString();
            jsonStr = jsonStr.Replace("\\", "/").Replace("//", "/");

            File.WriteAllText(jsonPath, jsonStr);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            manifestBundle.Unload(true);
            www.Dispose();
            www = null;

            AssetImporter ai = AssetImporter.GetAtPath(jsonPath);
            ai.assetBundleName = ABUtil.KEY_JSONNAME.ToLower();
            ai.assetBundleVariant = null;
            ai.SaveAndReimport();

            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
            #endregion

            #region Duplicated
            Dictionary<string, string> dicABName = new Dictionary<string, string>();
            Dictionary<string, List<string>> dicABDuplicated = new Dictionary<string, List<string>>();
            StringBuilder sb;
            for (int i = 0; i < dataList.Count; i++)
            {
                data = dataList[i];
                //Debug.Log(string.Format("{0} : {1}", data.name, data.assets.Length));
                for (int j = 0; j < data.assets.Length; j++)
                {
                    sb = new StringBuilder();
                    sb.Append(data.assets[j]);
                    if (!string.IsNullOrEmpty(data.variant))
                        sb.Append(".").Append(data.variant);

                    string assetName = sb.ToString();

                    if (dicABDuplicated.ContainsKey(assetName))
                    {
                        dicABDuplicated[assetName].Add(data.name);
                    }
                    else
                    {
                        if (dicABName.ContainsKey(assetName))
                        {
                            List<string> bundleNameList = new List<string>();
                            bundleNameList.Add(dicABName[assetName]);
                            bundleNameList.Add(data.name);
                            dicABDuplicated.Add(assetName, bundleNameList);

                            dicABName.Remove(assetName);
                        }
                        else
                        {
                            dicABName.Add(assetName, data.name);
                        }
                    }
                }
            }

            bool isDuplicated = false;
            sb = new StringBuilder();
            sb.Append("====================DUPLICATED AssetName List====================\r\n\r\n");
            foreach (string key in dicABDuplicated.Keys)
            {
                isDuplicated = true;
                sb.Append(key).Append("\t\tIS DUPLICATED AT ");
                for (int i = 0; i < dicABDuplicated[key].Count; i++)
                {
                    sb.Append(dicABDuplicated[key][i]);

                    if (i != dicABDuplicated[key].Count - 1)
                        sb.Append(" | ");
                }
                sb.Append("\r\n");
            }

            string logPath = PATH_DATA;
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
            logPath = Path.Combine(logPath, "duplicated_assetname_log.txt");
            Debug.Log("[logPath] " + logPath);

            File.WriteAllText(logPath, sb.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            #endregion

            if (showExplorer)
            {
                ShowExplorerOutputPath();

                if (isDuplicated) Application.OpenURL(logPath);
            }
        }

        public static void ShowExplorerOutputPath()
        {
            string outputPath = Path.Combine(ABUtil.PATH_OUTPUT, ABUtil.GetPlatformName());
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            ShowExplorer(outputPath);
        }

        private static void ShowExplorer(string itemPath)
        {
            itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
            System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
        }

        private static string GetBundlePath(string bundleName)
        {
            string manifestPath = Path.Combine(ABUtil.PATH_OUTPUT, ABUtil.GetPlatformName());
            manifestPath = Path.Combine(manifestPath, bundleName);
            string[] pathSplit = Application.dataPath.Split('/');
            manifestPath = Application.dataPath.Replace(pathSplit[pathSplit.Length - 1], manifestPath);

            return manifestPath;
        }

        private static string GetManifestPath()
        {
            return "file:///" + GetBundlePath(ABUtil.GetPlatformName()); ;
        }

        /*
        #region Checksum

        public class ChecksumData
        {
            public string name;
            public string checksum;
        }

        private static System.Security.Cryptography.MD5 md5 = null;
        public static string GetMD5Hash(string filePath)
        {
            if (null == md5)
                md5 = System.Security.Cryptography.MD5.Create();

            FileStream stream = File.OpenRead(filePath);
            string hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
            stream.Close();
            return hash;
        }

        private static void SaveChecksumJson()
        {
            string outputPath = Path.Combine(PATH_OUTPUT, ABUtil.GetPlatformName());

            //Make Json
            DirectoryInfo dir = new DirectoryInfo(outputPath);
            System.IO.FileInfo[] files = dir.GetFiles("*.*", SearchOption.AllDirectories);

            List<ChecksumData> checksumList = new List<ChecksumData>();
            string dirPath = dir.FullName + "\\";
            foreach (System.IO.FileInfo file in files)
            {
                if (".manifest" != file.Extension)
                {
                    ChecksumData data = new ChecksumData();
                    data.name = file.FullName.Replace(dirPath, string.Empty);
                    data.checksum = GetMD5Hash(file.FullName);
                    checksumList.Add(data);

                    //Debug.Log(data.key + "   " + data.value);

                    //using (ZipFile zip = new ZipFile())
                    //{
                    //    zip.AddFile(file.FullName);
                    //    zip.Save(file.FullName + ".zip");
                    //}
                }
            }

            string jsonPath = outputPath + "\\" + "checksum.json";
            Debug.Log(jsonPath);

            TextWriter tw = new StreamWriter(jsonPath);
            if (tw == null)
            {
                Debug.LogError("Cannot write to " + jsonPath);
                return;
            }

            JsonWriter jw = new JsonWriter();
            jw.PrettyPrint = true;
            JsonMapper.ToJson(checksumList, jw);
            string jsonStr = jw.ToString();
            jsonStr = jsonStr.Replace("\\", "/").Replace("//", "/");
            //Debug.Log(jsonStr);
            tw.Write(jsonStr);

            tw.Flush();
            tw.Close();
        }

        #endregion
        */
    }//class
}//namespace

