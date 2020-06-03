using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 扩展工具_打包资源
/// </summary>
public class Package_Editor
{
    [MenuItem("Build/打包AssetBundles")]
    static void PackageAssetBundles()
    {
        Debug.Log("hello world");
        string sourABPath = Application.dataPath + "/AssetBundles/";
        string desABPath = Application.dataPath + "/DestinationPath/Android/AssetBundles/";
        Directory.CreateDirectory(desABPath);
        //移动拷贝文件：目的去除.meta文件
        DirectoryInfo dirInfo = new DirectoryInfo(sourABPath);
        DirectoryInfo[] dirInfos = dirInfo.GetDirectories();
        for (int i = 0; i < dirInfos.Length; i++)
        {
            string tempPath = Path.Combine(desABPath, dirInfos[i].Name);
            Debug.Log(tempPath);
            DirectoryCopy(dirInfos[i].FullName, tempPath, true);
        }

        //压缩之前去除.meta文件
        DirectoryInfo desInfo = new DirectoryInfo(desABPath);
        FileInfo[] fileInfos = desInfo.GetFiles();
        for (int i = 0; i < fileInfos.Length; i++)
        {
            if (fileInfos[i].Name.Contains(".meta"))
            {
                File.Delete(fileInfos[i].FullName);
            }
        }
        //process压缩
        string zipPath = Application.dataPath + "/DestinationPath/AssetBundle.bat";
        ProcessAssetBundles(zipPath);
    }

    /// <summary>
    /// 启用wimrar压缩文件
    /// </summary>
    /// <param name="path">程序启动路径</param>
    static void ProcessAssetBundles(string path)
    {
        try
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = path;//dos命令启动winrar，看Process测试
            process.StartInfo.Arguments = string.Format("10");
            process.StartInfo.CreateNoWindow = false;//是否显示窗口
            process.Start();
            process.WaitForExit();
        }
        catch (System.Exception ex)
        {

            Debug.Log("process error" + ex);
        }
    }

    [MenuItem("Build/Process测试")]
    static void StartProcessTest()
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = @"E:\QQ浏览器\QQBrowser\QQBrowser.exe";
        process.StartInfo.Arguments = "https://www.baidu.com/?tn=78040160_5_pg&ch=9";
        process.Start();
        process.WaitForExit();
    }

    /// <summary>
    /// copy文件
    /// </summary>
    /// <param name="sourcePath">资源路径</param>
    /// <param name="desPath">目的路径</param>
    /// <param name="copySubDirs">是否包含子项</param>
    static void DirectoryCopy(string sourcePath, string desPath, bool copySubDirs)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);//目录
        DirectoryInfo[] dirInfos = dirInfo.GetDirectories();//子目录
        if (!dirInfo.Exists)
        {
            throw new DirectoryNotFoundException("can not find file" + sourcePath);
        }
        if (!Directory.Exists(desPath))
        {
            Directory.CreateDirectory(desPath);
        }
        FileInfo[] fileInfos = dirInfo.GetFiles();//获得目录下的文件
        for (int i = 0; i < fileInfos.Length; i++)
        {
            string tempPath = Path.Combine(desPath, fileInfos[i].Name);
            if (tempPath.Contains(".meta"))
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            else
            {
                fileInfos[i].CopyTo(tempPath,true);
            }
        }
        if (copySubDirs)//子目录继续复制
        {
            for (int i = 0; i < dirInfos.Length; i++)
            {
                string tempPath = Path.Combine(desPath, dirInfos[i].Name);
                DirectoryCopy(dirInfos[i].FullName, tempPath, true);
            }
        }
    }
}
