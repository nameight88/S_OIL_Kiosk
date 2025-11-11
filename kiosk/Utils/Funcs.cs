using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

/// <summary>
/// ÀÏ¹ÝÀûÀ¸·Î ¸¹ÀÌ »ç¿ëµÇ´Â À¯¿ëÇÑ ÇÔ¼öµéÀ» °¡Áö´Â Å¬·¡½º
/// </summary>
public partial class Funcs
{
    /// <summary>
    /// ¹ÙÀÌÆ® ¹è¿­À» String·Î º¯È¯ 
    /// </summary>
    /// <param name="strByte">¹ÙÀÌÆ® ¹è¿­</param>
    /// <param name="encoding">ÀÎÄÚµù</param>
    /// <returns>String</returns>
    static public string ByteToString(byte[] strByte, Encoding encoding)
    {
        if (strByte == null || strByte.Length < 1)
            return string.Empty;

        string str = encoding.GetString(strByte);
        return str;
    }
    /// <summary>
    /// StringÀ» ¹ÙÀÌÆ® ¹è¿­·Î º¯È¯ 
    /// </summary>
    /// <param name="str">String</param>
    /// <param name="encoding">ÀÎÄÚµù</param>
    /// <returns>¹ÙÀÌÆ® ¹è¿­</returns>
    static public byte[] StringToByte(string str, Encoding encoding)
    {
        if (string.IsNullOrWhiteSpace(str))
            return null;

        byte[] StrByte = encoding.GetBytes(str);
        return StrByte;
    }

    /// <summary>
    /// ¹èÁ¤¹Ð ºÎµ¿ ¼Ò¼öÁ¡ ¼ýÀÚ°¡ À¯È¿ÇÑÁö Ã¼Å©ÇÏ°í ±× °ªÀ» ¹ÝÈ¯ÇÑ´Ù.
    /// </summary>
    /// <param name="val">¹èÁ¤¹Ð ºÎµ¿ ¼Ò¼öÁ¡ ¼ýÀÚ</param>
    /// <returns>À¯È¿ÇÑ ¼ýÀÚ¸é ±× °ªÀ» ¹ÝÈ¯ÇÏ°í, ±×·¸Áö¾ÊÀ¸¸é ¿µÀ» ¹ÝÈ¯ÇÑ´Ù.</returns>
    static public double EvaluateDoule(double val)
    {
        if (double.IsInfinity(val) || double.IsNaN(val))
            return 0;
        else
            return val;
    }

    /// <summary>
    /// File StreamÀ» ÀÌ¿ëÇÏ¿© ÆÄÀÏÀ» Byte ¹è¿­·Î º¯È¯
    /// </summary>
    /// <param name="path">ÆÄÀÏ°æ·Î</param>
    /// <returns>Byte ¹è¿­</returns>
    static public byte[] FileToByteArray(string path)
    {
        byte[] fileBytes = null;
        try
        {
            if (path.Length > 0)
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    fileBytes = new byte[fileStream.Length];
                    fileStream.Read(fileBytes, 0, fileBytes.Length);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return fileBytes;
    }

    /// <summary>
    /// FileStreamÀ» »ç¿ëÇÏ¿© Byte ¹è¿­À» ÆÄÀÏ·Î º¯È¯
    /// </summary>
    /// <param name="path">ÆÄÀÏ°æ·Î</param>
    /// <param name="buffer">Byte ¹è¿­</param>
    /// <returns>¼º°øÇÏ¸é true, ±×·¸Áö¾ÊÀ¸¸é false</returns>
    static public bool ByteArrayToFile(string path, byte[] buffer)
    {
        try
        {
            if (path.Length < 1)
                return false;

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return false;
    }

    /// <summary>
    /// File.WriteAllBytes¸¦ ÀÌ¿ëÇÏ¿© Byte ¹è¿­À» ÆÄÀÏ·Î º¯È¯
    /// </summary>
    /// <param name="path">ÆÄÀÏ°æ·Î</param>
    /// <param name="buffer">Byte ¹è¿­</param>
    /// <returns>¼º°øÇÏ¸é true, ±×·¸Áö¾ÊÀ¸¸é false</returns>
    static public bool ByteArrayToFile2(string path, byte[] buffer)
    {
        try
        {
            // ´ë»ó ÆÄÀÏÀÌ ÀÖÀ¸¸é µ¤¾î¾´´Ù.
            File.WriteAllBytes(path, buffer);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return false;
    }

    /// <summary>
    /// ¹®ÀÚ¿­¿¡¼­ ¼ýÀÚ¸¸ ÃßÃâÇÑ´Ù.
    /// </summary>
    /// <param name="sData">¹®ÀÚ¿­</param>
    /// <param name="pattern">¼ýÀÚ¿¡ ÇØ´çÇÏ´Â ¹®ÀÚÀÇ ÆÐÅÏ</param>
    /// <returns>ÆÐÅÏ¿¡ ÇØ´çÇÏ´Â ¹®ÀÚ¿­</returns>
    static public string ExtractNumber(string sData, string pattern)
    {
        string result = "";

        if (string.IsNullOrWhiteSpace(pattern))
            pattern = @"[^a-zA-Z0-9.()+-{}*/ ]";

        string replacement = "";
        result = System.Text.RegularExpressions.Regex.Replace(sData, pattern, replacement).Replace(",", "");
        return result;
    }
    /// <summary>
    /// ¹®ÀÚ¿­¿¡¼­ ±¸ºÐÀÚ¸¦ Æ÷ÇÔÇÑ ¼ø¼öÇÑ ¼ýÀÚ¸¸ ÃßÃâÇÑ´Ù.
    /// </summary>
    /// <param name="sData">¹®ÀÚ¿­</param>
    /// <returns>¹®ÀÚ¿­</returns>
    /// <remarks>
    /// [¿¹Á¦]
    ///		{À­ÂÊ(¸éÀû(1200.12)*µÎ²²(10.5))+¾Æ·¡ÂÊ(¸éÀû(1200.12)*µÎ²²(10.5))}/2 => {(1200.12)*(10.5))+(1200.12)*(10.5))}/2
    /// </remarks>
    static public string ExtractNumber(string sData)
    {
        if (string.IsNullOrWhiteSpace(sData))
            return "";
#if (DEBUG_)
            if (sData.Contains("["))
            { }
#endif
        string result = "";
        string specialchars = @".(){}+-*/ ";
        bool start = false;
        bool end = true;
        bool is_number = false;

        try
        {
            foreach (char ch in sData)
            {
                switch (ch)
                {
                    case '[':
                        is_number = false;
                        start = true;
                        end = false;
                        break;
                    case ']':
                        is_number = false;
                        start = false;
                        end = true;
                        break;
                    case ',':
                        if (!is_number)
                            result += string.Format("{0}", ch);
                        break;
                    default:
                        if (start && !end)
                        {
                        }
                        else if (specialchars.Contains(ch)
                            || ('0' <= ch && ch <= '9')
                            || ('a' <= ch && ch <= 'z')
                            || ('A' <= ch && ch <= 'Z'))
                        {
                            if (('0' <= ch && ch <= '9')
                                || ('a' <= ch && ch <= 'z')
                                || ('A' <= ch && ch <= 'Z')
                                || ch == '.')
                            {
                                if (!is_number)
                                    is_number = true;
                            }
                            else
                                is_number = false;
                            result += string.Format("{0}", ch);
                        }
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            result = result.Replace("()", "");
            if (IsNumber(result))
                return result;
            else
                return "";
        }
        else
            return result;
    }

    /// <summary>
    /// °ø¹é¹®ÀÚÀ» Á¦¿ÜÇÑ Æ¯¼ö¹®ÀÚ¿­À» Á¦°ÅÇÑ´Ù.
    /// </summary>
    /// <param name="sData">¹®ÀÚ¿­</param>
    /// <param name="sPattern">¹®ÀÚ¿­¿¡ ÀÖ´Â ¹®ÀÚÀÇ Çü½Ä</param>
    /// <param name="sReplacement">´ëÃ¼ÇÒ ¹®ÀÚ¿­</param>
    /// <returns>°ø¹é¹®ÀÚÀ» Á¦¿ÜÇÑ Æ¯¼ö¹®ÀÚ¿­À» Á¦°ÅÇÑ ¹®ÀÚ¿­</returns>
    static public string RemoveSpecialChar(string sData, string sPattern = @"[^a-zA-Z0-9°¡-ÆR ]", string sReplacement = "")
    {
        return System.Text.RegularExpressions.Regex.Replace(sData, sPattern, sReplacement, System.Text.RegularExpressions.RegexOptions.Singleline);
    }

    /// <summary>
    /// Æ¯¼ö¹®ÀÚ ¹× ¼ýÀÚ¸¦ Á¦¿ÜÇÑ ¹®ÀÚ¿­¸¸ ÃßÃâÇÑ´Ù.
    /// </summary>
    /// <param name="sData">¹®ÀÚ¿­</param>
    /// <returns>Æ¯¼ö¹®ÀÚ ¹× ¼ýÀÚ¸¦ Á¦¿ÜÇÑ ¹®ÀÚ¿­¸¸ ÃßÃâÇÑ ¹®ÀÚ¿­</returns>
    static public string ExtractString(string sData)
    {
        if (string.IsNullOrWhiteSpace(sData))
            return string.Empty;

        string sPattern = @"[^a-zA-Z°¡-ÆR ]";
        string sReplacement = "";
        return System.Text.RegularExpressions.Regex.Replace(sData, sPattern, sReplacement, System.Text.RegularExpressions.RegexOptions.Singleline);
    }
    /// <summary>
    /// ÁÖ¾îÁø ÆÐÅÏ¿¡ ÇØ´çÇÏ´Â ¹®ÀÚ¿­¸¸ ÃßÃâÇÑ´Ù.
    /// </summary>
    /// <param name="sData">¹®ÀÚ¿­</param>
    /// <param name="pattern">ÃßÃâÇÏ·Á°í ÇÏ´Â ¹®ÀÚÀÇ ÁýÇÕ</param>
    /// <returns>¹®ÀÚ¿­</returns>
    /// <remarks>ÆÐÅÏÀÌ ¾øÀ¸¸é ¿µ¹®ÀÚ¿Í ÇÑ±Û ¹× °ø¹é¹®ÀÚ¸¸ ÃßÃâÇÑ´Ù.</remarks>
    static public string ExtractString(string sData, string pattern)
    {
        if (string.IsNullOrWhiteSpace(sData))
            return string.Empty;

        if (string.IsNullOrWhiteSpace(pattern))
            pattern = @"[^a-zA-Z°¡-ÆR ]";
        string replacement = "";
        return System.Text.RegularExpressions.Regex.Replace(sData, pattern, replacement);
    }

    /// <summary>
    /// ¹®ÀÚ¿­ÀÌ ¼ýÀÚÇüÀÎÁö Ã¼Å©ÇÑ´Ù.
    /// </summary>
    /// <param name="letter">¹®ÀÚ¿­</param>
    /// <returns>¼ýÀÚÇüÀÌ¸é true, ±×·¸Áö¾ÊÀ¸¸é false</returns>
    static public bool IsNumber(string letter)
    {
        if (string.IsNullOrWhiteSpace(letter))
            return false;

        double d = 0;
        bool res = double.TryParse(letter, out d);
        return res;
    }
    /// <summary>
    /// ÀÚ·áÇüÀÌ ¼ýÀÚÇüÀÎÁö Ã¼Å©ÇÑ´Ù.
    /// </summary>
    /// <param name="type">ÀÚ·áÇü</param>
    /// <returns>¼ýÀÚÇüÀÌ¸é true, ±×·¸Áö¾ÊÀ¸¸é false</returns>
    static public bool IsNumber(Type type)
    {
        switch (type.ToString().ToLower())
        {
            case "system.int16":
            case "system.int32":
            case "system.int64":
            case "system.uint16":
            case "system.uint32":
            case "system.uint64":
            case "system.double":
            case "system.decimal":
                return true;
        }
        return false;
    }

    /// <summary>
    /// 10Áø¼ö¸¦ NÁø¼ö·Î º¯È¯ÇÏ´Â ÇÔ¼ö
    /// </summary>
    /// <param name="value">10Áø¼ö</param>
    /// <param name="N">Áø¼ö</param>
    /// <returns>NÁø¼ö·Î º¯È¯µÈ °ªÀÇ ¹®ÀÚ¿­</returns>
    static public string ConvertToNdigit(int value, int N = 26)
    {
        string result = "";
        int a = 0;
        int b = 0;
        List<int> values = new List<int>();

        try
        {
            do
            {
                a = value / N;
                b = value % N;
                values.Add(b);
                value = a;
            } while (a != 0);

            int i = 0;
            int val = 0;
            bool is_over = false;
            foreach (int v in values)
            {
                val = v;
                if (is_over)
                {
                    --val;
                    is_over = false;
                }

                if (val == 0)
                {
                    if (i < values.Count - 1)
                    {
                        result = string.Format("{0}{1}", "Z", result);
                        is_over = true;
                    }
                }
                else
                    result = string.Format("{0}{1}", (char)('A' + (val - 1)), result);
                ++i;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        return result;
    }

    /// <summary>
    /// ¹®ÀÚ¿­¿¡ ÁÖ¾îÁø °Ë»ö ¹®ÀÚ°¡ ÀÖ´ÂÁö Ã¼Å©
    /// </summary>
    /// <param name="find">°Ë»ö ¹®ÀÚ</param>
    /// <param name="bytes">¹®ÀÚ¿­</param>
    /// <returns>true=Á¸ÀçÇÔ</returns>
    static public bool IsContainsForBytes(byte[] find, byte[] bytes)
    {
        if (find == bytes)
            return true;

        if (find == null || bytes == null)
            return false;

        try
        {
            bool is_started = false;
            int match = 0;
            foreach (byte b in bytes)
            {
                if (b == find[0])
                {
                    if (!is_started)
                    {
                        is_started = true;
                        match = 0;
                    }
                    ++match;

                    if (match == find.Length)
                        return true;
                }
                else
                {
                    if (is_started)
                    {
                        is_started = false;
                        match = 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return false;
    }
    /// <summary>
    /// ¹®ÀÚ¿­¿¡ ÁÖ¾îÁø °Ë»ö ¹®ÀÚ°¡ ÀÖ´ÂÁö Ã¼Å©
    /// </summary>
    /// <param name="find">°Ë»ö ¹®ÀÚ</param>
    /// <param name="chars">¹®ÀÚ¿­</param>
    /// <returns>true=Á¸ÀçÇÔ</returns>
    static public bool IsContainsForChars(char[] find, char[] chars)
    {
        if (find == chars)
            return true;

        if (find == null || chars == null)
            return false;

        try
        {
            bool is_started = false;
            int match = 0;
            foreach (char b in chars)
            {
                if (b == find[match])
                {
                    if (!is_started)
                    {
                        is_started = true;
                        match = 0;
                    }
                    ++match;

                    if (match == find.Length)
                        return true;
                }
                else
                {
                    if (is_started)
                    {
                        is_started = false;
                        match = 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return false;
    }

    /// <summary>
    /// TÇü °³Ã¼°¡ TÇü ¸ñ·Ï¿¡ Á¸ÀçÇÏ´ÂÁö Ã¼Å©
    /// </summary>
    /// <typeparam name="T">ÀÚ·áÇü</typeparam>
    /// <param name="o">TÇü °³Ã¼</param>
    /// <param name="list">TÇü ¸ñ·Ï</param>
    /// <returns>true=Á¸ÀçÇÔ</returns>
    static public bool IsContains<T>(T o, List<T> list)
    {
        try
        {
            if (o != null && list.Count > 0)
                foreach (T it in list)
                {
                    if (it.Equals(o))
                        return true;
                }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return false;
    }
    /// <summary>
    /// TÇü °³Ã¼°¡ TÇü ¸ñ·Ï¿¡ Á¸ÀçÇÏ´ÂÁö Ã¼Å©
    /// </summary>
    /// <typeparam name="T">ÀÚ·áÇü</typeparam>
    /// <param name="list">TÇü ¸ñ·Ï</param>
    /// <param name="listItem">TÇü °³Ã¼</param>
    /// <returns>true=Á¸ÀçÇÔ</returns>
    static public bool IsContains<T>(List<List<T>> list, List<T> listItem)
    {
        listItem.Sort();

        for (int i = 0; i < list.Count; i++)
        {
            List<T> temp = list[i];

            if (temp.Count == listItem.Count)
            {
                temp.Sort();

                if (temp.SequenceEqual(listItem))
                {
                    return true;
                }

            }
        }

        return false;
    }

    /// <summary>
    ///  µÎ°³ÀÇ º¯¼öÀÇ °ªÀ» ¹Ù²Û´Ù.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    static public void Swap<T>(ref T a, ref T b)
    {
        T c = a;
        a = b;
        b = c;
    }

    /// <summary>
    /// »çºÐ±â¿¡¼­ ¾îµð¿¡ ¼ÓÇÏ´ÂÁö ±¸ÇÑ´Ù.
    /// </summary>
    /// <param name="angle">°¢µµ(Redius)</param>
    /// <returns>1=1»çºÐ±â,2=2»çºÐ±â,3=3»çºÐ±â,4=4»çºÐ±â</returns>
    static public double ToQuarter(double angle)
    {
        double result = 0;

        try
        {
            if (0 <= angle && angle <= Math.PI / 4)
                result = angle;
            else if (Math.PI / 4 <= angle && angle <= Math.PI / 2)
                result = Math.PI / 2 - angle;
            else if (Math.PI / 2 <= angle && angle <= Math.PI * 3 / 4)
                result = angle - Math.PI / 2;
            else //if (Math.PI / 4 <= angle && angle <= Math.PI / 2)
                result = Math.PI - angle;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return Math.Round(result, 7);
    }
    /// <summary>
    /// ÁÖ¾îÁø °¢µµ°¡ ¼öÆòÀÎÁö ¿©ºÎ ±¸ÇÏ±â
    /// </summary>
    /// <param name="angle">°¢µµ(radius)</param>
    /// <returns>¼öÆòÀÌ¸é true</returns>
    static public bool IsHorizon(double angle)
    {
        try
        {
            double quarter = ToQuarter(angle);
            if (0 <= quarter && quarter <= Math.PI / 8)
                return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return false;
    }
    /// <summary>
    /// ÁÖ¾îÁø °¢µµ°¡ ¼öÁ÷ÀÎÁö ¿©ºÎ ±¸ÇÏ±â
    /// </summary>
    /// <param name="angle">°¢µµ(radius)</param>
    /// <returns>¼öÁ÷ÀÌ¸é true</returns>
    static public bool IsVertical(double angle)
    {
        try
        {
            double quarter = ToQuarter(angle);
            if (Math.PI / 8 < quarter)
                return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return false;
    }

    /// <summary>
    /// µ¥ÀÌÅÍ¸¦ ¹®ÀÚ¿­·Î º¯È¯ÇÑ´Ù.
    /// </summary>
    /// <param name="value">µ¥ÀÌÅÍ</param>
    /// <returns>¹®ÀÚ¿­</returns>
    static public string OToString(object value)
    {
        try
        {
            if (value == null || value.GetType() == typeof(DBNull))
                return "";
            else if (value is string)
                return value as string;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        return value.ToString();
    }
    /// <summary>
    /// µ¥ÀÌÅÍ¸¦ 16ºñÆ® Á¤¼öÇüÀ¸·Î º¯È¯ÇÑ´Ù.
    /// </summary>
    /// <param name="value">µ¥ÀÌÅÍ</param>
    /// <returns>16ºñÆ® Á¤¼öÇü</returns>
    static public Int16 OToInt16(object value)
    {
        try
        {
            if (value == null || value.GetType() == typeof(DBNull))
                return 0;
            else if (value is string)
            {
                string number = ExtractNumber(value as string);
                if (!string.IsNullOrWhiteSpace(number))
                    return Convert.ToInt16(number);
                else
                    return 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return Convert.ToInt16(value);
    }
    /// <summary>
    /// µ¥ÀÌÅÍ¸¦ 32ºñÆ® Á¤¼öÇüÀ¸·Î º¯È¯ÇÑ´Ù.
    /// </summary>
    /// <param name="value">µ¥ÀÌÅÍ</param>
    /// <returns>32ºñÆ® Á¤¼öÇü</returns>
    static public Int32 OToInt32(object value)
    {
        if (value == null || value.GetType() == typeof(DBNull))
            return 0;
        else if (value is string)
        {
            string number = ExtractNumber(value as string);
            if (string.IsNullOrWhiteSpace(number))
                return 0;
            else
                return Convert.ToInt32(number);
        }
        else
            return Convert.ToInt32(value);
    }
    /// <summary>
    /// µ¥ÀÌÅÍ¸¦ 64ºñÆ® Á¤¼öÇüÀ¸·Î º¯È¯ÇÑ´Ù.
    /// </summary>
    /// <param name="value">µ¥ÀÌÅÍ</param>
    /// <returns>64ºñÆ® Á¤¼öÇü</returns>
    static public Int64 OToInt64(object value)
    {
        try
        {
            if (value == null || value.GetType() == typeof(DBNull))
                return 0;
            else if (value is string)
            {
                string number = ExtractNumber(value as string);
                if (!string.IsNullOrWhiteSpace(number))
                    return Convert.ToInt64(number);
                else
                    return 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        return Convert.ToInt64(value);
    }
    /// <summary>
    /// µ¥ÀÌÅÍ¸¦ ¹èÁ¤¹Ð ºÎµ¿ ¼Ò¼öÁ¡ ¼ýÀÚ·Î º¯È¯ÇÑ´Ù.
    /// </summary>
    /// <param name="value">µ¥ÀÌÅÍ</param>
    /// <returns>¹èÁ¤¹Ð ºÎµ¿ ¼Ò¼öÁ¡ ¼ýÀÚ</returns>
    static public double OToDouble(object value)
    {
        try
        {
            if (value == null || value.GetType() == typeof(DBNull))
                return 0;
            else if (value is string)
            {
                string number = ExtractNumber(value as string);
                if (!string.IsNullOrWhiteSpace(number))
                    return Convert.ToDouble(number);
                else
                    return 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        return Convert.ToDouble(value);
    }
    /// <summary>
    /// µ¥ÀÌÅÍ¸¦ Á¤¹Ð ºÎµ¿ ¼Ò¼öÁ¡ ¼ýÀÚ·Î º¯È¯ÇÑ´Ù.
    /// </summary>
    /// <param name="value">µ¥ÀÌÅÍ</param>
    /// <returns>Á¤¹Ð ºÎµ¿ ¼Ò¼öÁ¡ ¼ýÀÚ</returns>
    static public float OToFloat(object value)
    {
        try
        {
            if (value == null || value.GetType() == typeof(DBNull))
                return 0;
            else if (value is string)
            {
                string number = ExtractNumber(value as string);
                if (!string.IsNullOrWhiteSpace(number))
                    return (float)Convert.ToDouble(number);
                else
                    return 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        return (float)Convert.ToDouble(value);
    }
    /// <summary>
    /// µ¥ÀÌÅÍ¸¦ ³¯Â¥ÇüÀ¸·Î º¯È¯ÇÑ´Ù.
    /// </summary>
    /// <param name="value">µ¥ÀÌÅÍ</param>
    /// <returns>³¯Â¥Çü</returns>
    static public DateTime OToDateTime(object value)
    {
        try
        {
            if (value == null || value.GetType() == typeof(DBNull))
                return DateTime.MinValue;
            else if (value is string)
            {
                string date = value as string;
                DateTime dt;
                if (!string.IsNullOrWhiteSpace(date) && date.Length >= 10 && DateTime.TryParse(date, out dt))
                    return Convert.ToDateTime(date);
                else
                    return DateTime.MinValue;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        return Convert.ToDateTime(value);
    }

    /// <summary>
    /// Hex ¹®ÀÚ¿­À» 32ºñÆ® Á¤¼öÇüÀ¸·Î º¯È¯ÇÏ±â
    /// </summary>
    /// <param name="hexValue">Hex ¹®ÀÚ¿­</param>
    /// <returns>32ºñÆ® Á¤¼öÇü</returns>
    static public Int32 HexToInt32(string hexValue)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hexValue))
                return 0;
            else
                return int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return 0;
        }
    }

    /// <summary>
    /// Hex ¹®ÀÚ¿­À» 32ºñÆ® Á¤¼öÇüÀ¸·Î º¯È¯ÇÏ±â
    /// </summary>
    /// <param name="hexValue">Hex ¹®ÀÚ¿­</param>
    /// <returns>32ºñÆ® Á¤¼öÇü</returns>
    static public Int64 HexToInt64(string hexValue)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hexValue))
                return 0;
            else
                return Int64.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return 0;
        }
    }

    /// <summary>
    /// µÎ »çÀü°ªµéÀÌ µ¿ÀÏÇÑÁö Ã¼Å©ÇÑ´Ù.
    /// </summary>
    /// <typeparam name="TKey">Å°</typeparam>
    /// <typeparam name="TValue">°ª</typeparam>
    /// <param name="x">»çÀü°ªµé1</param>
    /// <param name="y">»çÀü°ªµé2</param>
    /// <returns>µ¿ÀÏÇÏ¸é Âü, ±×·¸Áö¾ÊÀ¸¸é °ÅÁþ</returns>
    public static bool Equals<TKey, TValue>(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
    {
        try
        {
            // early-exit checks
            if (null == y)
                return null == x;
            if (null == x)
                return false;
            if (object.ReferenceEquals(x, y))
                return true;
            if (x.Count != y.Count)
                return false;

            // check keys are the same
            foreach (TKey k in x.Keys)
                if (!y.ContainsKey(k))
                    return false;

            // check values are the same
            foreach (TKey k in x.Keys)
                if (!x[k].Equals(y[k]))
                    return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return true;
    }
    /// <summary>
    /// µÎ »çÀü°ªµéÀÌ ¾î´À ÇÏ³ª¿¡ Æ÷ÇÔµÇ´ÂÁö Ã¼Å©ÇÑ´Ù.
    /// </summary>
    /// <typeparam name="TKey">Å°</typeparam>
    /// <typeparam name="TValue">°ª</typeparam>
    /// <param name="x">»çÀü°ªµé1</param>
    /// <param name="y">»çÀü°ªµé2</param>
    /// <returns>¾î´À ÇÏ³ª¿¡ Æ÷ÇÔµÇ¸é Âü, ±×·¸Áö¾ÊÀ¸¸é °ÅÁþ</returns>
    public static bool Includes<TKey, TValue>(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
    {
        try
        {
            // early-exit checks
            if (null == y)
                return null == x;
            if (null == x)
                return false;
            if (object.ReferenceEquals(x, y))
                return true;

            if (0 == y.Count)
                return 0 == x.Count;
            if (0 == x.Count)
                return false;

            int n = Math.Min(x.Count, y.Count);
            IDictionary<TKey, TValue> u = x.Count < y.Count ? x : y;
            IDictionary<TKey, TValue> v = x.Count < y.Count ? y : x;

            foreach (KeyValuePair<TKey, TValue> pair in u)
            {
                if (!v.ContainsKey(pair.Key))
                    return false;
                else if (!pair.Value.Equals(v[pair.Key]))
                    return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return true;
    }
    /// <summary>
    /// µÎ »çÀü°ªµéÀÇ Å°µéÀÌ ¾î´À ÇÏ³ª¿¡ Æ÷ÇÔµÇ´ÂÁö Ã¼Å©ÇÑ´Ù.
    /// </summary>
    /// <typeparam name="TKey">Å°</typeparam>
    /// <typeparam name="TValue">°ª</typeparam>
    /// <param name="x">»çÀü°ªµé1</param>
    /// <param name="y">»çÀü°ªµé2</param>
    /// <returns>¾î´À ÇÏ³ª¿¡ Æ÷ÇÔµÇ¸é Âü, ±×·¸Áö¾ÊÀ¸¸é °ÅÁþ</returns>
    public static bool IncludesKey<TKey, TValue>(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
    {
        try
        {
            // early-exit checks
            if (null == y)
                return null == x;
            if (null == x)
                return false;
            if (object.ReferenceEquals(x, y))
                return true;

            if (0 == y.Count)
                return 0 == x.Count;
            if (0 == x.Count)
                return false;

            int n = Math.Min(x.Count, y.Count);
            IDictionary<TKey, TValue> u = x.Count < y.Count ? x : y;
            IDictionary<TKey, TValue> v = x.Count < y.Count ? y : x;

            foreach (KeyValuePair<TKey, TValue> pair in u)
            {
                if (!v.ContainsKey(pair.Key))
                    return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return true;
    }


    /// <summary>
    /// º¯¼öÀÇ ¸íÄªÀ» ±¸ÇÑ´Ù.
    /// </summary>
    /// <typeparam name="T">º¯¼öÀÇ À¯Çü</typeparam>
    /// <param name="variable">º¯¼ö</param>
    /// <returns>º¯¼öÀÇ ¸íÄª</returns>
    public static string GetVariableName<T>(T variable)
    {
        try
        {
            System.Linq.Expressions.Expression<Func<T>> expr = () => variable;
            var body = (System.Linq.Expressions.MemberExpression)expr.Body;

            return (body.Member.Name);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return "";
    }
    /// <summary>
    /// ÁöÁ¤µÈ °´Ã¼¿¡¼­ ÀúÁ¤µÈ º¯¼ö¸íÀ» °¡Áø º¯¼öÀÇ °ªÀ» ±¸ÇÑ´Ù.
    /// </summary>
    /// <param name="obj">°´Ã¼</param>
    /// <param name="varname">º¯¼ö¸í</param>
    /// <returns>º¯¼öÀÇ °ª</returns>
    public static object GetVariableValue(object obj, string varname)
    {
        try
        {
            if (obj != null && !string.IsNullOrWhiteSpace(varname))
            {
                Type type = obj.GetType();
                var fis = (from v in type.GetFields()
                           where v.Name.ToUpper() == varname.ToUpper()
                           select v);
                if (fis.Count() > 0)
                {
                    System.Reflection.FieldInfo fi = fis.First();
                    return fi.GetValue(obj);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return null;
    }


    /// <summary>
    /// ÁÖ¾îÁø ½Ã°£(milliseconds)¸¸Å­ ´ë±âÇÑ´Ù.
    /// </summary>
    /// <param name="MS">½Ã°£(milliseconds)</param>
    public static void Delay(int MS)
    {
        DateTime ThisMoment = DateTime.Now;
        TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
        DateTime AfterWards = ThisMoment.Add(duration);

        while (AfterWards >= ThisMoment)
        {
            Application.DoEvents();

            ThisMoment = DateTime.Now;
        }
    }

    /// <summary>
    /// ÁÖ¾îÁø ID¸¦ °¡Áø ÇÁ·Î¼¼½º¸¦ ±¸ÇÑ´Ù.
    /// - ÀÌ¹Ì Á¾·áµÈ ÇÁ·Î¼¼½º´Â ¾ø´Â °ÍÀ¸·Î Á¤ÀÇÇÑ´Ù.
    /// </summary>
    /// <param name="procid">ÇÁ·Î¼¼½º ID</param>
    /// <returns>ÇÁ·Î¼¼½º</returns>
    public static Process GetProcessById(int procid)
    {
        try
        {
            if (procid < 1)
                return null;

            Process[] allProc = Process.GetProcesses();
            Process proc = (from v in allProc
                            where v.Id == procid
                            select v).FirstOrDefault();
            if (proc != null)
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                    return proc;
                else
                {
                    proc.Dispose();
                    proc = null;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return null;
    }

    /// <summary>
    /// ÁÖ¾îÁø À©µµ¿ì Á¦¸ñÀ» °®´Â À©µµ¿ì ÇÚµéÀ» ±¸ÇÑ´Ù.
    /// </summary>
    /// <param name="captionname">À©µµ¿ì Á¦¸ñ</param>
    /// <returns>À©µµ¿ì ÇÚµé</returns>
    public static Process GetProcess(string captionname)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(captionname))
                return null;

            Process[] allProc = Process.GetProcesses();
            Process proc = (from v in allProc
                            where v.MainWindowTitle == captionname
                            select v).FirstOrDefault();
            return proc;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return null;
    }
    /// <summary>
    /// ÁÖ¾îÁø ÇÁ·Î¼¼½º¸íÀ» °®´Â À©µµ¿ì ÇÚµéÀ» ±¸ÇÑ´Ù.
    /// </summary>
    /// <param name="processname">ÇÁ·Î¼¼½º¸í</param>
    /// <returns>À©µµ¿ì ÇÚµé</returns>
    public static IntPtr GetWindowHandleByProcessName(string processname)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(processname))
                return IntPtr.Zero;

            Process[] allProc = Process.GetProcesses();
            Process proc = (from v in allProc
                            where v.ProcessName == processname
                            select v).FirstOrDefault();
            if (proc != null)
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                    return proc.MainWindowHandle;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("processname " + ex.ToString());
        }

        return IntPtr.Zero;
    }
    /// <summary>
    /// ÁÖ¾îÁø ÇÁ·Î¼¼½º¸íÀ» °®´Â ÇÁ·Î¼¼½ºÀ» ±¸ÇÑ´Ù.
    /// </summary>
    /// <param name="processname">ÇÁ·Î¼¼½º¸í</param>
    /// <returns>ÇÁ·Î¼¼½º</returns>
    public static Process GetProcessByProcessName(string processname)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(processname))
                return null;

            Process[] allProc = Process.GetProcesses();
            Process proc = (from v in allProc
                            where v.ProcessName == processname
                            select v).FirstOrDefault();
            if (proc != null)
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                    return proc;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("processname " + ex.ToString());
        }

        return null;
    }

    /// <summary>
    /// Àç»ýÁßÀÎ ¿µ»ó ÇÁ·Î¼¼½º¸¦ Á¾·áÇÑ´Ù.
    /// </summary>
    /// <param name="procid">Process ID</param>
    /// <param name="name">Process Name (default=Empty)</param>
    public static void KillProcess(int procid, string name = "")
    {
        try
        {
            if (procid > 0)
            {
                Process proc = GetProcessById(procid);
                if (proc != null)
                {
                    proc.Kill();
                    proc.WaitForExit();
                    //proc.Dispose();
                    //proc = null;
#if (DEBUG_)
                        System.Diagnostics.Debug.WriteLine("{0}[Process:{1}] kill.", name, procid);
#endif
                }
                else
                    System.Diagnostics.Debug.WriteLine(name + "[Process:" + procid + "] ½ÇÇàµÇ°í ÀÖÁö ¾Ê½À´Ï´Ù.");
                procid = 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Àç»ýÁßÀÎ ¿µ»ó ÇÁ·Î¼¼½º¸¦ Á¾·áÇÑ´Ù.
    /// </summary>
    /// <param name="procid">Process ID</param>
    /// <param name="name">Process Name</param>
    public static void CloseProcess(int procid, string name)
    {
        try
        {
            if (procid > 0)
            {
                Process proc = GetProcessById(procid);
                if (proc != null)
                {
                    proc.Close();
#if (DEBUG_)
                        System.Diagnostics.Debug.WriteLine("{0}[Process:{1}] Close.", name, procid);
#endif
                }
                else
                    System.Diagnostics.Debug.WriteLine(name + "[Process:" + procid + "] ½ÇÇàµÇ°í ÀÖÁö ¾Ê½À´Ï´Ù.");
                procid = 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// ÇöÀç »ç¿ëÁßÀÎ ·ÎÄ® ÄÄÇ»ÅÍÀÇ IP address¸¦ ±¸ÇÑ´Ù.
    /// </summary>
    /// <returns> IP address</returns>
    public static string GetLocalIP()
    {
        try
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return "";
    }

    /// <summary>
    /// ±ä ¹®ÀÚ¿­À» ÂªÀº ¹®ÀÚ¿­·Î º¯È¯ÇÑ´Ù.
    /// </summary>
    /// <param name="str">±ä ¹®ÀÚ¿­</param>
    /// <param name="length">ÂªÀº ¹®ÀÚ¿­ÀÇ ±æÀÌ (±âº»±æÀÌ=10¹ÙÀÌÆ®)</param>
    /// <returns>ÂªÀº ¹®ÀÚ¿­</returns>
    public static string ShortString(string str, int length = 20)
    {
        string result = str;

        try
        {
            var bytes = Encoding.Default.GetBytes(str);
            if (bytes.Length > length)
            {
                byte[] sub = new byte[length];
                Array.Copy(bytes, sub, length);
                result = string.Format("{0}...", Encoding.Default.GetString(sub));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return result;
    }

    /// <summary>
    /// ÀÎÄÚµù Å¸ÀÔ ±¸ÇÏ±â
    /// </summary>
    /// <param name="bomByteArray">BOM ¹ÙÀÌÆ® ¹è¿­</param>
    /// <returns>ÀÎÄÚµù Å¸ÀÔ</returns>
    public static string GetEncodingType(byte[] bomByteArray)
    {
        if ((bomByteArray[0] & 0xff) == 0xef && (bomByteArray[1] & 0xff) == 0xbb && (bomByteArray[2] & 0xff) == 0xbf)
        { return "utf-8"; }
        else if ((bomByteArray[0] & 0xff) == 0xfe && (bomByteArray[1] & 0xff) == 0xff)
        { return "utf-16be"; }
        else if ((bomByteArray[0] & 0xff) == 0xff && (bomByteArray[1] & 0xff) == 0xfe)
        { return "utf-16le"; }
        else if ((bomByteArray[0] & 0xff) == 0x00 && (bomByteArray[1] & 0xff) == 0x00 && (bomByteArray[0] & 0xff) == 0xfe && (bomByteArray[1] & 0xff) == 0xff)
        { return "utf-32be"; }
        else if ((bomByteArray[0] & 0xff) == 0xff && (bomByteArray[1] & 0xff) == 0xfe && (bomByteArray[0] & 0xff) == 0x00 && (bomByteArray[1] & 0xff) == 0x00)
        { return "utf-32le"; }
        else { return "euc-kr"; }
    }

    /// <summary>
    /// stringÀ» ASCII string À¸·Î º¯È¯
    /// </summary>
    /// <param name="value">¹®ÀÚ¿­</param>
    /// <returns>ASCII string</returns>
    public static string StringToAsciiString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
    }

    /// <summary>
    /// LParam ±¸ÇÏ±â
    /// </summary>
    /// <param name="LoWord">ÇÏÀ§ 2¹ÙÀÌÆ®</param>
    /// <param name="HiWord">»óÀ§ 2¹ÙÀÌÆ®</param>
    /// <returns></returns>
    public static int MakeLParam(int LoWord, int HiWord)
    {
        return (int)((HiWord << 16) | (LoWord & 0xFFFF));
    }

    /// <summary>
    /// ÆùÆ®¸¦ ¸Þ¸ð¸®¿¡¼­ ÇØÁ¦ÇÑ´Ù.
    /// </summary>
    /// <param name="font">ÆùÆ®</param>
    public static void Release(Font font)
    {
        if (font != null)
        {
            font.Dispose();
            font = null;
        }
    }

    /// <summary>
    /// ÁÖ¾îÁø »ç¿îµå¸¦ ÇÃ·¹ÀÌ ½ÃÀÛÇÏ±â
    /// </summary>
    /// <param name="sound">»ç¿îµå ÆÄÀÏ</param>
    public static void StartSoundPlay(string sound)
    {
        try
        {
            System.Threading.Thread th = new System.Threading.Thread(SoundPlay);
            th.Start(sound);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// ÁÖ¾îÁø »ç¿îµå¸¦ ÇÃ·¹ÀÌÇÏ±â
    /// </summary>
    /// <param name="data">»ç¿îµå ÆÄÀÏ</param>
    public static void SoundPlay(object data)
    {
        try
        {
            string sound = Funcs.OToString(data);
            if (!string.IsNullOrWhiteSpace(sound))
            {
                if (System.IO.File.Exists(sound))
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(sound);
                    player.LoadAsync();
                    player.Play();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// IP Á¤ÇÕ¼º Ã¼Å©ÇÏ±â
    /// </summary>
    /// <param name="addr">IP (¿¹:127.0.0.1)</param>
    /// <returns>À¯È¿ÇÑ IPÀÌ¸é true</returns>
    public static bool IsValidIP(string addr)
    {
        IPAddress ip;
        bool valid = !string.IsNullOrEmpty(addr) && IPAddress.TryParse(addr, out ip);
        return valid;
    }

    //µÎ DateTimeÀÇ ÀÏ¼ö Â÷ÀÌ¸¦ ±¸ÇÏ´Â ÇÔ¼ö
    public static int GetDaysBetweenDateTime(DateTime date1, DateTime date2)
    {
        int days = 0;

        if (date1.CompareTo(date2) < 0)
        {
            TimeSpan delta = date2 - date1;
            days = (int)delta.TotalDays;
        }
        else
        {
            TimeSpan delta = date1 - date2;
            days = (int)delta.TotalDays;
        }

        return days;
    }

    //µÎ DateTimeÀÇ ½Ã°£ Â÷ÀÌ¸¦ ±¸ÇÏ´Â ÇÔ¼ö
    public static int GetHoursBetweenDateTime(DateTime date1, DateTime date2)
    {
        int hours = 0;

        if (date1.CompareTo(date2) < 0)
        {
            TimeSpan delta = date2 - date1;
            hours = (int)delta.TotalHours;
        }
        else
        {
            TimeSpan delta = date1 - date2;
            hours = (int)delta.TotalHours;
        }

        return hours;
    }

    //µÎ DateTimeÀÇ ºÐ Â÷ÀÌ¸¦ ±¸ÇÏ´Â ÇÔ¼ö
    public static int GetMinutesBetweenDateTime(DateTime date1, DateTime date2)
    {
        int days = 0;

        if (date1.CompareTo(date2) < 0)
        {
            TimeSpan delta = date2 - date1;
            days = (int)delta.Minutes;
        }
        else
        {
            TimeSpan delta = date1 - date2;
            days = (int)delta.Minutes;
        }

        return days;
    }

    //µÎ DateTimeÀÇ ÃÊ Â÷ÀÌ¸¦ ±¸ÇÏ´Â ÇÔ¼ö
    public static int GetSecondsBetweenDateTime(DateTime date1, DateTime date2)
    {
        int days = 0;

        if (date1.CompareTo(date2) < 0)
        {
            TimeSpan delta = date2 - date1;
            days = (int)delta.TotalSeconds;
        }
        else
        {
            TimeSpan delta = date1 - date2;
            days = (int)delta.TotalSeconds;
        }

        return days;
    }

    //µÎ DateTimeÀÇ Â÷ÀÌ¸¦ ¹®ÀÚ¿­·Î ¹ÝÈ¯ÇÏ´Â ÇÔ¼ö
    public static string ConvertDeltaDateTimeToStr(DateTime date1, DateTime date2)
    {
        string dateStr = "";
        DateTime earlyDate = date1.CompareTo(date2) > 0 ? date2 : date1;
        DateTime lateDate = date1.CompareTo(date2) > 0 ? date1 : date2;

        int years = 0;
        int months = 0;
        int days = 0;

        for (var i = 1; ; ++i)
        {
            if (earlyDate.AddYears(i) > lateDate)
            {
                years = i - 1;

                break;
            }
        }

        for (var i = 1; ; ++i)
        {
            if (earlyDate.AddYears(years).AddMonths(i) > lateDate)
            {
                months = i - 1;

                break;
            }
        }

        for (var i = 1; ; ++i)
        {
            if (earlyDate.AddYears(years).AddMonths(months).AddDays(i) > lateDate)
            {
                days = i - 1;

                break;
            }
        }

        string yearStr = years == 0 ? "" : years + "³â ";
        string monthStr = months == 0 ? "" : months.ToString("D2") + "°³¿ù ";
        string dayStr = days.ToString("D2") + "ÀÏ";

        dateStr = yearStr + monthStr + dayStr;

        return dateStr;
    }

    public static int GetLastDateTime(DateTime date1, DateTime date2)
    {
        int res = -1;
        if (date1.CompareTo(date2) < 0)
        {
            res = 1;
        }
        else
        {
            res = 0;
        }
        return res;
    }
}