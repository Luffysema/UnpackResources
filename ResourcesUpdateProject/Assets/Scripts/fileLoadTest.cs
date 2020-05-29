using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class fileLoadTest : MonoBehaviour {

    /// <summary>进度条</summary>
    public Slider m_Slider;

    public string m_TestFilePath = "";

    void Start()
    {
        m_TestFilePath = Application.dataPath + "/WebResures";
        //测试下载文件
        //DoLoadRes();

        //测试解压文件
        TestUnZipFile();
        InvokeRepeating("RefreashUpdateState", 0.2f, 0);
    }

    /// <summary>
    /// 测试加载资源
    /// </summary>
    private void DoLoadRes()
    {
        //http和https的区别
        //Tools.GetInstance().DownLoadRes("http://saturngame-1251588552.cos.ap-guangzhou.myqcloud.com/kdjd/kdjd_and_res_v83.zip", m_TestFilePath);
    }

    /// <summary>
    /// 测试解压文件
    /// </summary>
    private void TestUnZipFile()
    {        
        string zipPath = m_TestFilePath;
        Tools.GetInstance().UnZipFile(zipPath, Application.dataPath);
    }

    /// <summary>
    /// 刷新更新状态（下载文件，更新文件）
    /// </summary>
    private void RefreashUpdateState()
    {
        if (m_Slider!=null)
        {
            m_Slider.value = Tools.GetInstance().m_UnZipPercent;
        }
    }

}
