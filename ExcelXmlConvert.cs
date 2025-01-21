using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Data;
using System.Text;
using System;

public class ExcelXmlConvert
{
    public class ExcelDataCallbackValue
    {
        public string sheetName;
        public int row;
        public int column;
        public string data;
        public string dataType;
    }

    public delegate void MyExcelDataCallback(ExcelDataCallbackValue value);

    public static bool ConvertExcelTable(string fileName, Stream stream, MyExcelDataCallback callback, string sheetName)
    {
        XmlDocument doc = new XmlDocument();
        using (XmlTextReader reader = new XmlTextReader(stream))
        {
            doc.Load(reader);
        }

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("o", "urn:schemas-microsoft-com:office:office");
        nsmgr.AddNamespace("x", "urn:schemas-microsoft-com:office:excel");
        nsmgr.AddNamespace("ss", "urn:schemas-microsoft-com:office:spreadsheet");

        ExcelDataCallbackValue val = new ExcelDataCallbackValue();

        foreach (XmlNode node in doc.DocumentElement.SelectNodes("//ss:Worksheet", nsmgr))
        {
            val.sheetName = node.Attributes["ss:Name"].Value;
            XmlNodeList rows = node.SelectNodes("ss:Table/ss:Row", nsmgr);

            val.row = 0; // 1-based index.
            for (int r = 0; r < rows.Count; ++r)
            {
                if (rows[r].Attributes["ss:Index"] != null)
                    val.row = int.Parse(rows[r].Attributes["ss:Index"].Value);
                else
                    val.row++;

                XmlNodeList cells = rows[r].SelectNodes("ss:Cell", nsmgr);
                val.column = 0; // 1-based index
                for (int c = 0; c < cells.Count; ++c)
                {
                    XmlNode cell = cells[c];
                    if (cell.Attributes["ss:Index"] != null)
                        val.column = int.Parse(cell.Attributes["ss:Index"].Value);
                    else
                        val.column++;

                    XmlNode innerData = cell.SelectSingleNode("ss:Data", nsmgr);
                    if (innerData != null)
                    {
                        val.data = innerData.InnerText;
                        val.dataType = (innerData.Attributes["ss:Type"] != null) ? innerData.Attributes["ss:Type"].Value : null;

                        // process only when no sheet name is specified ( = process all sheet)
                        // or given sheet name matches current cell's sheet name.
                        if (sheetName == null ||
                            sheetName == val.sheetName)
                        {
                            try
                            {
                                callback(val);                                
                            }
                            catch (Exception ex)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    public static bool ConvertExcelTable(string fileName, MyExcelDataCallback callback, string sheetName)
    {
        bool succeeded = true;
        FileStream stream = null;
        try
        {
            stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (System.Exception e)
        {
            e.ToString();
            stream = null;
        }

        if (stream == null)
        {
            try
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Write);
            }
            catch (System.Exception e)
            {
                e.ToString();
                stream = null;
            }
        }

        if (stream == null)
        {
            //Debug.LogError("Cannot open " + fileName);
            succeeded = false;
        }
        else
        {
            using (stream)
            {
                succeeded = ConvertExcelTable(fileName, stream, callback, sheetName);
            }
            stream.Dispose();
        }

        return succeeded;
    }

    //public static void exportToExcel(DataTable source, [Optional] int nHdnColumnStart, [Optional] int nLookupSrcColStart, [Optional] List<int> lstLookupCounts, [Optional] List<int> lstDestLookupCols)
    /// <summary>
    /// Creates Excel Worksheet for the provided Datatable
    /// </summary>
    /// <param name="source"></param>
    /// <param name="nHdnColumnStart"></param>
    /// <param name="nLookupSrcColStart"></param>
    /// <param name="lstLookupCounts"></param>
    /// <param name="lstDestLookupCols"></param>
    public static StringBuilder ExportExcelTable(List<DataTable> sheets, List<string> sheetNames)
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            const string startExcelXML = "<?xml version=\"1.0\"?>\r\n" +
            "<?mso-application progid=\"Excel.Sheet\"?>\r\n<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n" +
            "xmlns:o=\"urn:schemas-microsoft-com:office:office\"\r\n xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\r\n" +
            "xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n xmlns:html=\"http://www.w3.org/TR/REC-html40\">\"\r\n" +
            "<DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">\r\n<Author>147943</Author>\r\n" +
            "<LastAuthor>151864</LastAuthor> \r\n <Created>2007-12-27T08:49:22Z</Created>\r\n <LastSaved>2008-01-02T10:24:54Z</LastSaved> \r\n <Version>12.00</Version>\r\n </DocumentProperties>\r\n" +
            " <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\"> \r\n <WindowHeight>9210</WindowHeight> \r\n <WindowWidth>11355</WindowWidth> \r\n <WindowTopX>480</WindowTopX>\r\n" +
            "<WindowTopY>60</WindowTopY> \r\n <TabRatio>309</TabRatio> \r\n <ProtectStructure>False</ProtectStructure>\r\n <ProtectWindows>False</ProtectWindows>\r\n </ExcelWorkbook>\r\n" +
            "<Styles> \r\n <Style ss:ID=\"Default\" ss:Name=\"Normal\"> \r\n <Alignment ss:Vertical=\"Bottom\"/> \r\n <Borders/> \r\n <Font ss:FontName=\"맑은 고딕\" x:Family=\"Swiss\"/>\r\n <Interior/>\r\n" +
            "<NumberFormat/> \r\n <Protection/> \r\n </Style> \r\n <Style ss:ID=\"s62\"> \r\n <Alignment ss:Vertical=\"Bottom\"/> \r\n <Borders/> \r\n <Font ss:FontName=\"Arial\" x:Family=\"Swiss\" ss:Color=\"#000000\"/>" +
            "</Style> \r\n <Style ss:ID=\"s63\"> \r\n <Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Bottom\"/><Borders/> \r\n <Font ss:FontName=\"Arial\" x:Family=\"Swiss\" ss:Color=\"#000000\" ss:Bold=\"1\"/>" +
            "<Interior ss:Color=\"#FAC090\" ss:Pattern=\"Solid\"/> \r\n </Style> \r\n</Styles>\r\n";

            sb.Append(startExcelXML);
            const string endExcelXML = "</Workbook>";
            for(int sheetCount = 0; sheetCount < sheets.Count; ++sheetCount)
            {
                int rowCount = 0;
                int ColumnStart = 0;
                sb.Append("<Worksheet ss:Name=\"" + sheetNames[sheetCount] + "\">\r\n");
                sb.Append("<Table x:FullColumns=\"1\" x:FullRows=\"1\">\r\n");

                //To Hide Columns
                if (ColumnStart > 0)
                    sb.Append("<Column ss:Index=\"" + ColumnStart + "\" ss:Hidden=\"1\" ss:AutoFitWidth=\"0\" ss:Span=\"" + sheets[sheetCount].Columns.Count + "\"/>\r\n");

                sb.Append("<Row ss:StyleID=\"s62\">\r\n");

                //To be commented start
                for (int x = 0; x < sheets[sheetCount].Columns.Count; x++)
                {
                    sb.Append("<Cell ss:StyleID=\"s63\"><Data ss:Type=\"String\">");
                    sb.Append(sheets[sheetCount].Columns[x].ColumnName);
                    sb.Append("</Data></Cell>\r\n");
                }
                sb.Append("</Row>\r\n");

                foreach (DataRow x in sheets[sheetCount].Rows)
                {
                    rowCount++;
                    //if the number of rows is > 64000 create a new page to continue output
                    //if (rowCount == 64000)
                    //{
                    //    rowCount = 0;
                    //    sheetCount++;
                    //    sb.Append("</Table>\r\n");
                    //    sb.Append("</Worksheet>\r\n");
                    //    sb.Append("<Worksheet ss:Name=\"" + sheetNames[sheetCount] + "\">\r\n");
                    //    sb.Append("<Table>\r\n");
                    //}
                    sb.Append("<Row>\r\n"); //ID=" + rowCount + "

                    for (int nActuals = 0; nActuals < sheets[sheetCount].Columns.Count; nActuals++)
                    {
                        sb.Append("<Cell>");
                        System.Type rowType;
                        rowType = x[nActuals].GetType();
                        string XMLstring = x[nActuals].ToString();
                        XMLstring = XMLstring.Trim();
                        XMLstring = XMLstring.Replace("&", "&");
                        XMLstring = XMLstring.Replace(">", ">");
                        XMLstring = XMLstring.Replace("<", "<");

                        if (rowType.ToString() == "System.Int16" || rowType.ToString() == "System.Int32" || rowType.ToString() == "System.Int64" || rowType.ToString() == "System.Byte")
                            sb.Append("<Data ss:Type=\"Number\">");
                        //else if(rowType.ToString() == "System.DateTime")
                        //    sb.Append("<Data ss:Type=\"DateTime\">");
                        else
                            sb.Append("<Data ss:Type=\"String\">");

                        ///////////// 특수 문자 예외처리
                        if (XMLstring.Contains("&"))
                            XMLstring = XMLstring.Replace("&", "&amp;");

                        if (XMLstring.Contains("<"))
                            XMLstring = XMLstring.Replace("<", "&lt;");

                        if (XMLstring.Contains(">"))
                            XMLstring = XMLstring.Replace("<", "&gt;");

                        if (nActuals >= ColumnStart - 1)
                        {
                            if (!string.IsNullOrEmpty(XMLstring))
                                sb.Append(XMLstring);
                        }
                        else
                            sb.Append(XMLstring);

                        sb.Append("</Data></Cell>\r\n");
                    }
                    sb.Append("</Row>\r\n");
                }

                sb.Append("</Table>\r\n");
                sb.Append("<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">\r\n");
                sb.Append("<Selected/>\r\n");
                sb.Append("<ProtectObjects>False</ProtectObjects>\r\n");
                sb.Append("<ProtectScenarios>False</ProtectScenarios>\r\n");
                sb.Append("</WorksheetOptions>\r\n");


                sb.Append("</Worksheet>\r\n");
            }
            sb.Append(endExcelXML);
            return sb;
        }
        //catch (Exception ex)
        catch
        {
            throw;
        }
    }

    public static StringBuilder ExportExcelTable(List<KeyValuePair<string, DataTable>> sheetPairs)
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            const string startExcelXML = "<?xml version=\"1.0\"?>\r\n" +
            "<?mso-application progid=\"Excel.Sheet\"?>\r\n<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n" +
            "xmlns:o=\"urn:schemas-microsoft-com:office:office\"\r\n xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\r\n" +
            "xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n xmlns:html=\"http://www.w3.org/TR/REC-html40\">\"\r\n" +
            "<DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">\r\n<Author>147943</Author>\r\n" +
            "<LastAuthor>151864</LastAuthor> \r\n <Created>2007-12-27T08:49:22Z</Created>\r\n <LastSaved>2008-01-02T10:24:54Z</LastSaved> \r\n <Version>12.00</Version>\r\n </DocumentProperties>\r\n" +
            " <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\"> \r\n <WindowHeight>9210</WindowHeight> \r\n <WindowWidth>11355</WindowWidth> \r\n <WindowTopX>480</WindowTopX>\r\n" +
            "<WindowTopY>60</WindowTopY> \r\n <TabRatio>309</TabRatio> \r\n <ProtectStructure>False</ProtectStructure>\r\n <ProtectWindows>False</ProtectWindows>\r\n </ExcelWorkbook>\r\n" +
            "<Styles> \r\n <Style ss:ID=\"Default\" ss:Name=\"Normal\"> \r\n <Alignment ss:Vertical=\"Bottom\"/> \r\n <Borders/> \r\n <Font ss:FontName=\"맑은 고딕\" x:Family=\"Swiss\"/>\r\n <Interior/>\r\n" +
            "<NumberFormat/> \r\n <Protection/> \r\n </Style> \r\n <Style ss:ID=\"s62\"> \r\n <Alignment ss:Vertical=\"Bottom\"/> \r\n <Borders/> \r\n <Font ss:FontName=\"Arial\" x:Family=\"Swiss\" ss:Color=\"#000000\"/>" +
            "</Style> \r\n <Style ss:ID=\"s63\"> \r\n <Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Bottom\"/><Borders/> \r\n <Font ss:FontName=\"Arial\" x:Family=\"Swiss\" ss:Color=\"#000000\" ss:Bold=\"1\"/>" +
            "<Interior ss:Color=\"#FAC090\" ss:Pattern=\"Solid\"/> \r\n </Style> \r\n</Styles>\r\n";

            sb.Append(startExcelXML);
            const string endExcelXML = "</Workbook>";
            for (int sheetCount = 0; sheetCount < sheetPairs.Count; ++sheetCount)
            {
                var SheetName = sheetPairs[sheetCount].Key;
                var Sheet = sheetPairs[sheetCount].Value;
                int rowCount = 0;
                int ColumnStart = 0;
                sb.Append("<Worksheet ss:Name=\"" + SheetName + "\">\r\n");
                sb.Append("<Table x:FullColumns=\"1\" x:FullRows=\"1\">\r\n");

                //To Hide Columns
                if (ColumnStart > 0)
                    sb.Append("<Column ss:Index=\"" + ColumnStart + "\" ss:Hidden=\"1\" ss:AutoFitWidth=\"0\" ss:Span=\"" + Sheet.Columns.Count + "\"/>\r\n");

                sb.Append("<Row ss:StyleID=\"s62\">\r\n");

                //To be commented start
                for (int x = 0; x < Sheet.Columns.Count; x++)
                {
                    sb.Append("<Cell ss:StyleID=\"s63\"><Data ss:Type=\"String\">");
                    sb.Append(Sheet.Columns[x].ColumnName);
                    sb.Append("</Data></Cell>\r\n");
                }
                sb.Append("</Row>\r\n");

                foreach (DataRow x in Sheet.Rows)
                {
                    rowCount++;
                    //if the number of rows is > 64000 create a new page to continue output
                    //if (rowCount == 64000)
                    //{
                    //    rowCount = 0;
                    //    sheetCount++;
                    //    sb.Append("</Table>\r\n");
                    //    sb.Append("</Worksheet>\r\n");
                    //    sb.Append("<Worksheet ss:Name=\"" + sheetNames[sheetCount] + "\">\r\n");
                    //    sb.Append("<Table>\r\n");
                    //}
                    sb.Append("<Row>\r\n"); //ID=" + rowCount + "

                    for (int nActuals = 0; nActuals < Sheet.Columns.Count; nActuals++)
                    {
                        sb.Append("<Cell>");
                        System.Type rowType;
                        rowType = x[nActuals].GetType();
                        string XMLstring = x[nActuals].ToString();
                        XMLstring = XMLstring.Trim();
                        XMLstring = XMLstring.Replace("&", "&");
                        XMLstring = XMLstring.Replace(">", ">");
                        XMLstring = XMLstring.Replace("<", "<");

                        if (rowType.ToString() == "System.Int16" || rowType.ToString() == "System.Int32" || rowType.ToString() == "System.Int64" || rowType.ToString() == "System.Byte")
                            sb.Append("<Data ss:Type=\"Number\">");
                        //else if(rowType.ToString() == "System.DateTime")
                        //    sb.Append("<Data ss:Type=\"DateTime\">");
                        else
                            sb.Append("<Data ss:Type=\"String\">");

                        ///////////// 특수 문자 예외처리
                        if (XMLstring.Contains("&"))
                            XMLstring = XMLstring.Replace("&", "&amp;");

                        if (XMLstring.Contains("<"))
                            XMLstring = XMLstring.Replace("<", "&lt;");

                        if (XMLstring.Contains(">"))
                            XMLstring = XMLstring.Replace("<", "&gt;");

                        if (nActuals >= ColumnStart - 1)
                        {
                            if (!string.IsNullOrEmpty(XMLstring))
                                sb.Append(XMLstring);
                        }
                        else
                            sb.Append(XMLstring);

                        sb.Append("</Data></Cell>\r\n");
                    }
                    sb.Append("</Row>\r\n");
                }

                sb.Append("</Table>\r\n");
                sb.Append("<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">\r\n");
                sb.Append("<Selected/>\r\n");
                sb.Append("<ProtectObjects>False</ProtectObjects>\r\n");
                sb.Append("<ProtectScenarios>False</ProtectScenarios>\r\n");
                sb.Append("</WorksheetOptions>\r\n");


                sb.Append("</Worksheet>\r\n");
            }
            sb.Append(endExcelXML);
            return sb;
        }
        catch (Exception /*e*/)
        {
            throw;
        }
    }

    static string GetExcelColumnName(int col)
    {
        if (col <= 26)
        {
            char[] ch = new char[1];
            ch[0] = (char)(col + 'A' - 1);
            return new string(ch);
        }
        else
        {
            char[] ch = new char[2];
            ch[0] = (char)(col / 26 + 'A' - 1);
            ch[1] = (char)(col % 26 + 'A' - 1);
            return new string(ch);
        }
    }
}


public class TableBindHelper : TableElementSerializer
{
    private static uint[] crctab = new uint[]
    {
        0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
        0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
        0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
        0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
        0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
        0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
        0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
        0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
        0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
        0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
        0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
        0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
        0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
        0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
        0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
        0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
        0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
        0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
        0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
        0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
        0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
        0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
        0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
        0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
        0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
        0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
        0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
        0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
        0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
        0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
        0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
        0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
        0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
        0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
        0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
        0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
        0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
        0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
        0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
        0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
        0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
        0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
        0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
        0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
        0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
        0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
        0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
        0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
        0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
        0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
        0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
        0x2d02ef8d
    };

    int attributeIndex;
    int[] indexRef;
    ExcelXmlConvert.ExcelDataCallbackValue excelData;
    bool isHeaderRow;
    HashSet<string> excelHeaderItem = new HashSet<string>();
    HashSet<string> userHeaderItem = new HashSet<string>();
    bool isDebug = false;

    public TableBindHelper(int maxColumnNum, bool debug = false)
    {
        indexRef = new int[maxColumnNum + 10];
        isDebug = debug;
    }

    public void Begin(ExcelXmlConvert.ExcelDataCallbackValue val, bool isHeaderRow_)
    {
        if (isHeaderRow_)
        {
            val.data = val.data.Replace("#", "").Replace("*", "").Replace("★", "").Replace("$", "");
        }
        attributeIndex = 0;
        excelData = val;
        isHeaderRow = isHeaderRow_;
    }

    public void End()
    {
    }

    public void DumpMissingColumns(string tableName)
    {
        //MonoBehaviour.print("userHeaderItem"); foreach (string str in userHeaderItem) { MonoBehaviour.print("  " + str); }
        //MonoBehaviour.print("excelHeaderItem"); foreach (string str in excelHeaderItem) { MonoBehaviour.print("  " + str); }

        userHeaderItem.ExceptWith(excelHeaderItem);
        if (userHeaderItem.Count > 0)
        {
            string msg = "Missing columns found in " + tableName;
            foreach (string str in userHeaderItem)
            {
                msg += ", \"" + str + "\"";
            }
            // MonoBehaviour.print(msg);
        }
    }

    bool HandleHeaderRow(string columnName)
    {
        if (isHeaderRow)
        {
            if (excelData.data == columnName)
            {
                indexRef[attributeIndex] = excelData.column;
                //MonoBehaviour.print("header registered(int) : " + columnName + ", value : " + excelData.data);
            }

            if (excelData.data != null)
                excelHeaderItem.Add(excelData.data);
            if (columnName != null)
                userHeaderItem.Add(columnName);

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsBinaryLoading() { return false; }

    public void Bind(ref string outval, string columnName)
    {
        if (isHeaderRow)
        {
            if (isDebug)
            {
                // 임시 코드. 클래스에 string을 선언하고 초기값을 따로 지정하지 않아 null인 경우는
                // 여기에서 empty string으로 대입해 준다.
                // binary에서 읽는 경우는 BinaryWriter에서 null string을 empty string으로 변환해 저장하기 때문에
                // table element의 string 멤버는 항상 null이 아니다.
                if (outval == null) // Assign empty string to null string.
                    outval = "";
            }

            HandleHeaderRow(columnName);
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data != null)
                {
                    if (columnName.ToLower().StartsWith("t_"))
                    {
                        //if (string.Equals(excelData.data, "은은한 침묵의 링"))
                        //{
                        //    ulong va = 0;
                        //    {
                        //        int hash = excelData.data.Length;
                        //        for (int i = 0; i < excelData.data.Length; ++i)
                        //        {
                        //            char ch = excelData.data[i];
                        //            int ich = (int)ch;
                        //            int k_0 = ch & 0xFF;
                        //            int k_1 = (ch & 0xFF00) >> 8;

                        //            hash = (int)((hash >> 8) ^ crctab[(hash & 0xff) ^ k_0]);
                        //            hash = (int)((hash >> 8) ^ crctab[(hash & 0xff) ^ k_1]);
                        //        }
                        //        va = (ulong)hash;
                        //    }
                        //    ulong vc = 0;
                        //    {
                        //        ulong hashedValue = 3074457345618258791ul;
                        //        for (int i = 0; i < excelData.data.Length; i++)
                        //        {
                        //            hashedValue += excelData.data[i];
                        //            hashedValue *= 3074457345618258799ul;
                        //        }
                        //        vc = hashedValue;
                        //    }
                        //    ulong a = (va << 32) | vc;
                        //    outval = a.ToString("X");
                        //}
                        outval = excelData.data.Trim();
                    }
                    else
                    {
                        outval = excelData.data.Trim();
                    }
                }
            }
        }
        attributeIndex++;
    }

    public void Bind(ref int outval, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (!string.IsNullOrEmpty(excelData.data.Trim()))
                {
                    outval = int.Parse(excelData.data.Trim());
                    //try
                    //{
                    //	//MonoBehaviour.print("found match(int) : " + columnName + ", value : " + excelData.data);
                    //	outval = int.Parse(excelData.data);
                    //}
                    //catch (System.Exception e)
                    //{
                    //	e.ToString();
                    //	outval = 0;
                    //}
                }
                else
                    outval = 0;
            }
        }
        attributeIndex++;
    }

    public void Bind(ref uint outval, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval = uint.Parse(excelData.data.Trim());
                    //try
                    //{
                    //	//MonoBehaviour.print("found match(int) : " + columnName + ", value : " + excelData.data);
                    //	outval = uint.Parse(excelData.data);
                    //}
                    //catch (System.Exception e)
                    //{
                    //	e.ToString();
                    //	outval = 0;
                    //}
                }
                else
                    outval = 0;
            }
        }
        attributeIndex++;
    }

    public void Bind(ref long outval, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval = long.Parse(excelData.data.Trim());
                    //try
                    //{
                    //	//MonoBehaviour.print("found match(int) : " + columnName + ", value : " + excelData.data);
                    //	outval = long.Parse(excelData.data);
                    //}
                    //catch (System.Exception e)
                    //{
                    //	e.ToString();
                    //	outval = 0;
                    //}
                }
                else
                    outval = 0;
            }
        }
        attributeIndex++;
    }

    public void Bind(ref float outval, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval = float.Parse(excelData.data.Trim());
                    //try
                    //{
                    //	//MonoBehaviour.print("found match(float) : " + columnName + ", value : " + excelData.data);
                    //	outval = float.Parse(excelData.data);
                    //}
                    //catch (System.Exception e)
                    //{
                    //	e.ToString();
                    //	outval = 0;
                    //}
                }
                else
                    outval = 0;
            }
        }
        attributeIndex++;
    }

    public void Bind(ref byte outval, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval = byte.Parse(excelData.data.Trim());
                    //try
                    //{
                    //	//MonoBehaviour.print("found match(float) : " + columnName + ", value : " + excelData.data);
                    //	outval = byte.Parse(excelData.data);
                    //}
                    //catch (System.Exception e)
                    //{
                    //	e.ToString();
                    //	outval = 0;
                    //}
                }
                else
                    outval = 0;
            }
        }
        attributeIndex++;
    }

    public void Bind(ref short outval, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval = short.Parse(excelData.data.Trim());
                }
                else
                    outval = 0;
            }
        }
        attributeIndex++;
    }

    public void Bind(ref ushort outval, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval = ushort.Parse(excelData.data.Trim());
                }
                else
                    outval = 0;
            }
        }
        attributeIndex++;
    }
    public void Bind(ref bool outval, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval = bool.Parse(excelData.data.Trim());
                }
                else
                    outval = false;
            }
        }
        attributeIndex++;
    }

    public void Bind(ref BitPacker outval) { }
    public void Bind(ref UIntPacker outval) { }
    public void Bind(ref FloatPacker outval) { }

    public void Bind(ref BitPacker outval, int index, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval[index] = byte.Parse(excelData.data.Trim());
                }
                else
                {
                    outval[index] = 0;
                }
            }
        }
        attributeIndex++;
    }

    public void Bind(ref UIntPacker outval, UIntPackerInfo info, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval[info] = uint.Parse(excelData.data.Trim());
                }
                else
                {
                    outval[info] = 0;
                }
            }
        }
        attributeIndex++;
    }

    public void Bind(ref FloatPacker outval, FloatPackerInfo info, string columnName)
    {
        if (HandleHeaderRow(columnName))
        {
        }
        else
        {
            if (excelData.column == indexRef[attributeIndex])
            {
                if (excelData.data.Trim() != null)
                {
                    outval[info] = float.Parse(excelData.data.Trim());
                }
                else
                {
                    outval[info] = 0;
                }
            }
        }
        attributeIndex++;
    }

    //public void Bind(ref ByteCompactor outval)
    //{
    //}

    //public void Bind(ref ByteCompactor outval, int index, string columnName)
    //{
    //}
}

public class TableConverterBase<T> where T : TableElement, new()
{
    int headerRowIndex;
    int maxNumColumns = 300;
    TableBindHelper helper;
    //TableElementRangeChecker rangeChecker = new TableElementRangeChecker();

    private T dummy = new T();
    public List<T> mydata = new List<T>();
    public bool mydataValid = false;
    //bool forceBuildUsingCShapCode = false;
    //bool forLoadOnly = false;
    //bool loadDataAfterBuildByExternalProcess = false;

    public TableConverterBase(int headerRowIndex_, string xmlName)
    {
        headerRowIndex = headerRowIndex_;
        //maxNumColumns = 300;
        helper = new TableBindHelper(maxNumColumns);
        ConvertTable(xmlName, null);
        //rangeChecker.Report(xmlName);
    }

    //public TableConverterBase(int headerRowIndex_, string xmlName, GameManager.TableNameEnum binaryNameEnum, int maxNumColumns_)
    //{
    //	headerRowIndex = headerRowIndex_;
    //	maxNumColumns = maxNumColumns_;
    //	helper = new TableBindHelper(maxNumColumns);
    //	ConvertTable(xmlName, binaryNameEnum, null);
    //}

    public TableConverterBase(int headerRowIndex_, string xmlName, string sheetName)//, bool buildUsingCShapCode_ = false, bool loadDataAfterBuildByExternalProcess_ = false)
    {
        headerRowIndex = headerRowIndex_;
        //maxNumColumns = 300;
        helper = new TableBindHelper(maxNumColumns);
        //		buildUsingCShapCode = buildUsingCShapCode_;
        //		loadDataAfterBuildByExternalProcess = loadDataAfterBuildByExternalProcess_;
        ConvertTable(xmlName, sheetName);
        //rangeChecker.Report(xmlName);
    }

    // binary로 변환하지 않고 로딩만 하는 경우 사용.
    public TableConverterBase(int headerRowIndex_, string xmlName, bool buildUsingCShapCode_ = true)
    {
        headerRowIndex = headerRowIndex_;
        //maxNumColumns = 300;
        helper = new TableBindHelper(maxNumColumns);
        //forceBuildUsingCShapCode = buildUsingCShapCode_;
        //forLoadOnly = true;
        //if (!forceBuildUsingCShapCode)
        //    loadDataAfterBuildByExternalProcess = true;
        ConvertTable(xmlName, null);
    }

    public TableConverterBase(int headerRowIndex_, string xmlName, string sheetName, bool buildUsingCShapCode_ = true)
    {
        headerRowIndex = headerRowIndex_;
        //maxNumColumns = 300;
        helper = new TableBindHelper(maxNumColumns);
        //forceBuildUsingCShapCode = buildUsingCShapCode_;
        //forLoadOnly = true;
        //if (!forceBuildUsingCShapCode)
        //    loadDataAfterBuildByExternalProcess = true;
        ConvertTable(xmlName, sheetName);
    }

    public TableConverterBase(int headerRowIndex_, string fileName, Stream stram, string sheetName, bool buildUsingCShapCode_ = true)
    {
        headerRowIndex = headerRowIndex_;
        //maxNumColumns = 300;
        helper = new TableBindHelper(maxNumColumns);
        //forceBuildUsingCShapCode = buildUsingCShapCode_;
        //forLoadOnly = true;
        //if (!forceBuildUsingCShapCode)
        //    loadDataAfterBuildByExternalProcess = true;
        ConvertTable(fileName, stram, sheetName);
    }

    public void Done() { }

    void OnSerializeHeader(TableElementSerializer helper)
    {
        dummy.Serialize(helper);
    }

    void OnSerializeData(TableElementSerializer helper, int index)
    {
        if (mydata.Count <= index)
        {
            while (mydata.Count <= index)
            {
                mydata.Add(new T());
            }
        }
        T data = mydata[index];
        data.Serialize(helper);
        //data.Serialize(rangeChecker);
    }

    int OnGetDataCount() { return mydata.Count; }

    void TableCallback(ExcelXmlConvert.ExcelDataCallbackValue val)
    {
        if (val.row < headerRowIndex)
            return;

        bool isHeaderRow = (val.row <= headerRowIndex) ? true : false;

        helper.Begin(val, isHeaderRow);

        if (isHeaderRow)
        {
            OnSerializeHeader(helper);
        }
        else
        {
            int index = val.row - headerRowIndex - 1; // 0-based index
            OnSerializeData(helper, index);
        }

        helper.End();
    }

    // binary로 변환하지 않고 로딩만 하는 경우 사용.
    void ConvertTable(string xmlName, string sheetName)
    {
        bool loaded = ExcelXmlConvert.ConvertExcelTable(xmlName, TableCallback, sheetName);
        if (loaded)
        {
            helper.DumpMissingColumns(xmlName);
            mydataValid = true;
        }
        else
        {
            //Debug.LogError("ExcelXmlConvert : cannot load \"" + xmlName + "\".");
            mydata.Clear();
            mydataValid = false;
        }
    }

    public static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    void ConvertTable(string fileName, Stream stream, string sheetName)
    {
        bool loaded = ExcelXmlConvert.ConvertExcelTable(fileName, stream, TableCallback, sheetName);
        if (loaded)
        {
            helper.DumpMissingColumns("binary table");
            mydataValid = true;
        }
        else
        {
            //Debug.LogError("ExcelXmlConvert : cannot load \"" + xmlName + "\".");
            mydata.Clear();
            mydataValid = false;
        }
    }
}
