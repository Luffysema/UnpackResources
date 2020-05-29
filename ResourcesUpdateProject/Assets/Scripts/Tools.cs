using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

/// <summary>
/// 工具类
/// </summary>
public class Tools
{
    public class Url
    {
        public string m_Url;
        /// <summary>
        /// 文件大小
        /// </summary>
        public long m_FileSize;
        /// <summary>
        /// 已经接收的长度
        /// </summary>
        public long m_ReceLength;
        /// <summary>
        /// 下载路径
        /// </summary>
        public string m_CurrentPath;
    }
    /// <summary>
    /// 私有化构造函数
    /// </summary>
    private Tools()
    { }

    /// <summary>
    /// 单例
    /// </summary>
    private static Tools m_Instance = new Tools();
    public static Tools GetInstance()
    {
        return m_Instance;
    }
    /// <summary>解压百分比</summary>
    public float m_UnZipPercent = 0f;
    /// <summary>
    /// 压缩文件路径
    /// </summary>
    private string m_ZipPath;
    /// <summary>
    /// 解压目标位置
    /// </summary>
    private string m_TargetPath;
    /// <summary>
    /// 当前下载的链接
    /// </summary>
    private Url m_CurrentUrl = new Url();
    /// <summary>
    /// 获取外部资源路径（可读，可写,"数据目录"）
    /// </summary>
    /// <returns></returns>
    public string GetOuterResPath()
    {
        string m_OuterResPath =
#if UNITY_EDTIOR
            Application.dataPath;
#else
            Application.persistentDataPath;
#endif
        return m_OuterResPath;
    }

    /// <summary>
    /// 获取内部资源路径（只可读，“游戏资源路径”）
    /// </summary>
    /// <returns></returns>
    public string GetInnerResPath()
    {
        string m_InnerrResPath =
#if UNITY_EDTIOR
            Applicaton.streamingAssetsPath;
#elif UNITY_ANDROID
            "jar:file://" + Application.dataPath + "!/assets/";
#else
            Application.dataPath + "/Raw";
#endif
        return m_InnerrResPath;
    }
    /// <summary>
    /// 彩色log
    /// </summary>
    /// <param name="str"></param>
    public void DebugColorLog(string str)
    {
#if UNITY_EDITOR
        Debug.Log("<color=#E37614>" + str + "</color>");
#endif
    }
    /// <summary>
    /// 解压文件
    /// </summary>
    /// <param name="zipPath">被解压的文件路径</param>
    /// <param name="targetPath">目标路径</param>
    public void UnZipFile(string zipPath, string targetPath)
    {
        m_ZipPath = zipPath;
        m_TargetPath = targetPath;
        m_UnZipPercent = 0f;
        ThreadStart s = new ThreadStart(_UnZipFile);
        Thread t = new Thread(s);
        t.Start();
    }

    /// <summary>
    /// 解压文件
    /// </summary>
    private void _UnZipFile()
    {
        if (string.IsNullOrEmpty(m_ZipPath) == true || string.IsNullOrEmpty(m_TargetPath) == true)
        {
            return;
        }
        DebugColorLog("ZipPath=" + m_ZipPath + "..." + "TargetPath=" + m_TargetPath);
        //
        long unZipFileSize = GetUnZipFileSize(m_ZipPath);
        //压缩流   
        ZipInputStream s = new ZipInputStream(File.OpenRead(m_ZipPath)); //File.OpenRead()  打开指定文件并读取
        if (s == null || unZipFileSize == 0)
        {
            DebugColorLog("压缩流s为空对应路径：" + m_ZipPath + "文件长度：" + unZipFileSize);
            return;
        }
        ZipEntry theEntry;
        int size = 2048;
        byte[] data = new byte[2048];
        long writeBytes = 0;
        try
        {
            while ((theEntry = s.GetNextEntry()) != null)
            {
                //注意文件名和目录名要区分
                string fileName = Path.GetFileName(theEntry.Name);
                DebugColorLog(theEntry.Name + "..." + fileName);
                if (string.IsNullOrEmpty(fileName) == false)
                {
                    string filePath = m_TargetPath + "/" + theEntry.Name;
                    FileStream streamWrite = new FileStream(filePath, FileMode.Create);
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            writeBytes += size;
                            m_UnZipPercent = writeBytes / unZipFileSize;
                            streamWrite.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWrite.Close();
                }
                else
                {
                    //创建多级文件（目录）
                    string filePath = m_TargetPath + "/" + theEntry.Name;
                    if (Directory.Exists(filePath) == false)
                    {
                        Directory.CreateDirectory(filePath);
                    }
                }
            }
        }
        catch (System.Exception)
        {

            throw;
        }
        s.Close();
    }

    /// <summary>
    /// 获取未解压文件长度
    /// </summary>
    /// <param name="zipPath">被解压的文件路径</param>
    private long GetUnZipFileSize(string zipPath)
    {
        long fileSize = 0;
        if (string.IsNullOrEmpty(zipPath) == false)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPath));
            if (s != null)
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (string.IsNullOrEmpty(fileName) == false)
                    {
                        fileSize += theEntry.Size;
                    }
                }

                s.Close();
            }
        }
        return fileSize;
    }


    /// <summary>
    /// 从服务器下载热更资源
    /// </summary>
    /// <param name="url"></param>
    /// <param name="targetPath"></param>
    public void DownLoadRes(string url, string targetPath)
    {
        m_CurrentUrl.m_Url = url;
        m_CurrentUrl.m_CurrentPath = targetPath;
        ThreadStart ts = new ThreadStart(_DownLoadRes);
        Thread t = new Thread(ts);
        t.Start();
    }

    /// <summary>
    /// 下载资源
    /// </summary>
    private void _DownLoadRes()
    {
        long tempLength = 0;
        FileStream fs;
        //存在表示已经下载一部分文件（断开的重连的情况）
        if (File.Exists(m_CurrentUrl.m_CurrentPath))
        {
            fs = File.OpenWrite(m_CurrentUrl.m_CurrentPath);
            tempLength = fs.Length;
            fs.Seek(tempLength, SeekOrigin.Current);
        }
        else
        {
            fs = new FileStream(m_CurrentUrl.m_CurrentPath, FileMode.Create);
        }
        m_CurrentUrl.m_ReceLength = tempLength;
        try
        {
            //获取下载链接，下载包地址
            HttpWebRequest w = (HttpWebRequest)HttpWebRequest.Create(new Uri(m_CurrentUrl.m_Url));

            if (tempLength > 0)
            {
                w.AddRange((int)tempLength);
            }
            Stream ns = w.GetResponse().GetResponseStream();
            m_CurrentUrl.m_FileSize = w.GetResponse().ContentLength + tempLength;

            byte[] datas = new byte[2048];
            int size = 0;
            size = ns.Read(datas, 0, datas.Length);
            while (size > 0)
            {
                m_CurrentUrl.m_ReceLength += size;

                fs.Write(datas, 0, size);
                size = ns.Read(datas, 0, datas.Length);
            }
            ns.Close();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        fs.Close();
    }
}
