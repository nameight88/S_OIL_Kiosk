using System;
using System.Text;

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

/// <summary>
/// ����� ���Ŀ� ���� ���
/// </summary>
public enum SaveMethod : int
{
    /// <summary>
    /// ������ ����
    /// </summary>
    None = 0,
    /// <summary>
    /// ���� ����
    /// </summary>
    Section = 1,
    /// <summary>
    /// Ű ����
    /// </summary>
    Key = 2,
    /// <summary>
    /// ����-Ű ����
    /// </summary>
    SectionKey = 3,
}

/// <summary>
/// INI ���� �ļ� �������̽�
/// </summary>
[ComVisible(true)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IIniParser
{
    /// <summary>
    /// INI������ �����ߴ����� ����
    /// </summary>
    bool IsNull();

    /// <summary>
    /// Opens the INI file at the given path and enumerates the values in the IniParser.
    /// </summary>
    /// <param name="iniPath">Full path to INI file.</param>
    /// <param name="to_upper">��� ������ �빮�ڷ� �ν������� ����</param>
    bool Open(string iniPath, bool to_upper = false);
    /// <summary>
    /// Returns the value for the given section, key pair.
    /// </summary>
    /// <param name="sectionName">Section name.</param>
    /// <param name="settingName">Key name.</param>
    /// <param name="initValue">�ʱⰪ</param>
    string GetSetting(string sectionName, string settingName, string initValue = "");
    /// <summary>
    /// Enumerates all lines for given section.
    /// </summary>
    /// <param name="sectionName">Section to enum.</param>
    string[] EnumSection(string sectionName);
    /// <summary>
    /// Adds or replaces a setting to the table to be saved.
    /// </summary>
    /// <param name="sectionName">Section to add under.</param>
    /// <param name="settingName">Key name to add.</param>
    /// <param name="settingValue">Value of key.</param>
    void AddSetting(string sectionName, string settingName, string settingValue);
    /// <summary>
    /// Adds or replaces a setting to the table to be saved with a null value.
    /// </summary>
    /// <param name="sectionName">Section to add under.</param>
    /// <param name="settingName">Key name to add.</param>
    void AddSetting(string sectionName, string settingName);
    /// <summary>
    /// Remove a setting.
    /// </summary>
    /// <param name="sectionName">Section to add under.</param>
    /// <param name="settingName">Key name to add.</param>
    void DeleteSetting(string sectionName, string settingName);
    /// <summary>
    /// Save settings to new file.
    /// </summary>
    /// <param name="newFilePath">New file path.</param>
    void SaveSettings(string newFilePath);
    /// <summary>
    /// Save settings back to ini file.
    /// </summary>
    void SaveSettings(SaveMethod method = SaveMethod.None);
    void SaveSettings(string newFilePath, SaveMethod method);
}

/// <summary>
/// INI ���� �ļ�
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class IniParser : IIniParser
{
    private Hashtable KeyPairs = new Hashtable();
    private string IniFilePath = "";

    /// <summary>
    /// ��ҹ��� ������ �ϴ����� ����
    /// </summary>
    private bool ToCase = false;

    /// <summary>
    /// INI������ �����ߴ����� ����
    /// </summary>
    public bool IsOpened = false;

    private struct SectionPair
    {
        public string Section;
        public string Key;
    }

    /// <summary>
    /// Opens the INI file at the given path and enumerates the values in the IniParser.
    /// </summary>
    /// <param name="iniPath">Full path to INI file.</param>
    /// <param name="to_upper">��� ������ �빮�ڷ� �ν������� ����</param>
    public IniParser(string iniPath, bool to_upper = false)
    {
        try
        {
            IsOpened = Open(iniPath, to_upper);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Opens the INI file at the given path and enumerates the values in the IniParser.
    /// </summary>
    /// <param name="iniPath">Full path to INI file.</param>
    /// <param name="to_upper">��� ������ �빮�ڷ� �ν������� ����</param>
    public bool Open(string iniPath, bool to_upper = false)
    {
        TextReader iniFile = null;
        string strLine = null;
        string currentRoot = null;
        string[] keyPair = null;

        IniFilePath = iniPath;
        ToCase = to_upper;

        if (!string.IsNullOrWhiteSpace(iniPath) && File.Exists(iniPath))
        {
            try
            {
                iniFile = new StreamReader(iniPath, Encoding.Default);

                strLine = iniFile.ReadLine();

                while (strLine != null)
                {
                    if (to_upper)
                        strLine = strLine.Trim().ToUpper();
                    else
                        strLine = strLine.Trim();

                    if (!string.IsNullOrWhiteSpace(strLine))
                    {
                        if (strLine[0] == ';' || strLine.Substring(0, 2) == "//")
                        {// �ּ� ó��
                            strLine = iniFile.ReadLine();
                            continue;
                        }

                        if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                        {
                            currentRoot = strLine.Substring(1, strLine.Length - 2);
                        }
                        else
                        {
                            keyPair = strLine.Split(new char[] { '=' }, 2);

                            SectionPair sectionPair;
                            string value = null;

                            if (currentRoot == null)
                                currentRoot = "ROOT";

                            sectionPair.Section = currentRoot;
                            sectionPair.Key = keyPair[0].Trim();

                            if (keyPair.Length > 1)
                                value = keyPair[1].Trim();

                            KeyPairs.Add(sectionPair, value);
                        }
                    }

                    strLine = iniFile.ReadLine();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                if (iniFile != null)
                    iniFile.Close();
            }
        }
        else
            System.Diagnostics.Debug.WriteLine("Unable to locate " + iniPath);

        return false;
    }

    /// <summary>
    /// INI������ �����ߴ����� ����
    /// </summary>
    /// <returns>INI������ �����ߴ����� ����</returns>
    public bool IsNull()
    {
        return IsOpened == false;
    }

    /// <summary>
    /// Returns the value for the given section, key pair.
    /// </summary>
    /// <param name="sectionName">Section name.</param>
    /// <param name="settingName">Key name.</param>
    /// <param name="initValue">�ʱⰪ</param>
    public string GetSetting(string sectionName, string settingName, string initValue = "")
    {
        try
        {
            SectionPair sectionPair;
            sectionPair.Section = ToCase ? sectionName.ToUpper() : sectionName;
            sectionPair.Key = ToCase ? settingName.ToUpper() : settingName;

            string sRet = (string)KeyPairs[sectionPair];
            if (string.IsNullOrWhiteSpace(sRet))
                return initValue;
            else
                return sRet;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        return string.Empty;
    }

    /// <summary>
    /// Enumerates all lines for given section.
    /// </summary>
    /// <param name="sectionName">Section to enum.</param>
    public string[] EnumSection(string sectionName)
    {
        ArrayList tmpArray = new ArrayList();

        foreach (SectionPair pair in KeyPairs.Keys)
        {
            if (ToCase)
            {
                if (pair.Section.ToUpper() == sectionName.ToUpper())
                    tmpArray.Add(pair.Key);
            }
            else
            {
                if (pair.Section == sectionName)
                    tmpArray.Add(pair.Key);
            }
        }

        return (string[])tmpArray.ToArray(typeof(string));
    }

    /// <summary>
    /// Adds or replaces a setting to the table to be saved.
    /// </summary>
    /// <param name="sectionName">Section to add under.</param>
    /// <param name="settingName">Key name to add.</param>
    /// <param name="settingValue">Value of key.</param>
    public void AddSetting(string sectionName, string settingName, string settingValue)
    {
        try
        {
            SectionPair sectionPair;
            sectionPair.Section = ToCase ? sectionName.ToUpper() : sectionName;
            sectionPair.Key = ToCase ? settingName.ToUpper() : settingName;

            if (KeyPairs.ContainsKey(sectionPair))
                KeyPairs[sectionPair] = settingValue;
            //KeyPairs.Remove(sectionPair);
            else
                KeyPairs.Add(sectionPair, settingValue);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Adds or replaces a setting to the table to be saved with a null value.
    /// </summary>
    /// <param name="sectionName">Section to add under.</param>
    /// <param name="settingName">Key name to add.</param>
    public void AddSetting(string sectionName, string settingName)
    {
        AddSetting(sectionName, settingName, null);
    }

    /// <summary>
    /// Remove a setting.
    /// </summary>
    /// <param name="sectionName">Section to add under.</param>
    /// <param name="settingName">Key name to add.</param>
    public void DeleteSetting(string sectionName, string settingName)
    {
        try
        {
            SectionPair sectionPair;
            sectionPair.Section = ToCase ? sectionName.ToUpper() : sectionName;
            sectionPair.Key = ToCase ? settingName.ToUpper() : settingName;

            if (KeyPairs.ContainsKey(sectionPair))
                KeyPairs.Remove(sectionPair);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Save settings to new file.
    /// </summary>
    /// <param name="newFilePath">New file path.</param>
    public void SaveSettings(string newFilePath)
    {
        ArrayList sections = new ArrayList();
        string tmpValue = "";
        string strToSave = "";

        foreach (SectionPair sectionPair in KeyPairs.Keys)
        {
            if (!sections.Contains(sectionPair.Section))
                sections.Add(sectionPair.Section);
        }

        foreach (string section in sections)
        {
            strToSave += ("[" + section + "]\r\n");

            foreach (SectionPair sectionPair in KeyPairs.Keys)
            {
                if (sectionPair.Section == section)
                {
                    tmpValue = (string)KeyPairs[sectionPair];

                    if (tmpValue != null)
                        tmpValue = "=" + tmpValue;

                    strToSave += (sectionPair.Key + tmpValue + "\r\n");
                }
            }

            strToSave += "\r\n";
        }

        try
        {
            Stream fs = new FileStream(newFilePath, FileMode.Create, FileAccess.Write);
            TextWriter tw = new StreamWriter(fs, Encoding.Default);
            tw.Write(strToSave);
            tw.Close();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// Save settings to new file.
    /// </summary>
    /// <param name="newFilePath">New file path.</param>
    /// <param name="method">�����ϱ� ���� �����ϴ� ���</param>
    public void SaveSettings(string newFilePath, SaveMethod method)
    {
        ArrayList sections = new ArrayList();
        string tmpValue = "";
        string strToSave = "";

        List<SectionPair> list;

        if (method == SaveMethod.SectionKey)
            list = (from SectionPair v in KeyPairs.Keys
                    orderby v.Section ascending, v.Key ascending
                    select v).ToList();
        else if (method == SaveMethod.Section)
            list = (from SectionPair v in KeyPairs.Keys
                    orderby v.Section ascending
                    select v).ToList();
        else
            list = (from SectionPair v in KeyPairs.Keys
                    select v).ToList();

        foreach (SectionPair sectionPair in list)
        {
            if (!sections.Contains(sectionPair.Section))
                sections.Add(sectionPair.Section);
        }

        foreach (string section in sections)
        {
            strToSave += ("[" + section + "]\r\n");

            List<SectionPair> selected;
            if (method == SaveMethod.SectionKey)
                selected = (from SectionPair v in KeyPairs.Keys
                            where v.Section == section
                            orderby v.Key ascending
                            select v).ToList();
            else
                selected = (from SectionPair v in KeyPairs.Keys
                            where v.Section == section
                            select v).ToList();
            foreach (var v in selected)
            {
                {
                    tmpValue = (string)KeyPairs[v];

                    if (tmpValue != null)
                        tmpValue = "=" + tmpValue;

                    strToSave += (v.Key + tmpValue + "\r\n");
                }
            }

            strToSave += "\r\n";
        }

        try
        {
            Stream fs = new FileStream(newFilePath, FileMode.Create, FileAccess.Write);
            TextWriter tw = new StreamWriter(fs, Encoding.Default);
            tw.Write(strToSave);
            tw.Close();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Save settings back to ini file.
    /// </summary>
    /// <param name="method">���Ĺ��</param>
    public void SaveSettings(SaveMethod method = SaveMethod.None)
    {
        SaveSettings(IniFilePath, method);
    }
}
