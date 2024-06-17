using AdminInterfase.MoreWindow;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.Devices;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.IO;
using NPOI.XWPF.UserModel;
using System.IO;
using NPOI.XWPF.UserModel;
using NPOI.OpenXmlFormats.Wordprocessing;
using Microsoft.Win32;
using System.Windows;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static AdminInterfase.MainWindow;

namespace AdminInterfase
{
    public class ExportManager
    {
        public static void ExportToExcelSessionInfo(List<SessionInfo> sessionInfos, string filePath,string startData, string endData)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("SessionInfo");

                string header = string.IsNullOrEmpty(startData) || string.IsNullOrEmpty(endData)
                    ? "Отчёт о времени использования компьютеров"
                    : $"Отчёт о времени использования компьютеров с {startData} по {endData}";

                ws.Cells["A1:E1"].Merge = true;
                ws.Cells["A1:E1"].Value = header;
                ws.Cells["A1:E1"].Style.Font.Size = 20;
                ws.Cells["A1:E1"].Style.Font.Bold = true;
                ws.Cells["A1:E1"].Style.Font.Italic = true;
                ws.Cells["A1:E1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                ws.Cells["A2"].Value = "Имя компьютера";
                ws.Cells["B2"].Value = "Пользователь";
                ws.Cells["C2"].Value = "Событие";
                ws.Cells["D2"].Value = "Дата/Время";
                ws.Cells["E2"].Value = "ОС";

                ws.Cells["A2:E2"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["A2:E2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);
                ws.Cells["A2:E2"].Style.Font.Color.SetColor(System.Drawing.Color.White);
                ws.Cells["A2:E2"].Style.Font.Bold = true;

                int rowStart = 3;
                foreach (var item in sessionInfos)
                {
                    ws.Cells[string.Format("A{0}", rowStart)].Value = item.computerName;
                    ws.Cells[string.Format("B{0}", rowStart)].Value = item.user;
                    ws.Cells[string.Format("C{0}", rowStart)].Value = item.OSEevent;
                    ws.Cells[string.Format("C{0}", rowStart)].Style.Font.Bold = true;
                    ws.Cells[string.Format("D{0}", rowStart)].Value = item.date;
                    ws.Cells[string.Format("E{0}", rowStart)].Value = item.os;

                    if (item.OSEevent == "Завершение работы")
                    {
                        ws.Cells[string.Format("C{0}", rowStart)].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    }
                    else if (item.OSEevent == "Запуск")
                    {
                        ws.Cells[string.Format("C{0}", rowStart)].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                    }

                    rowStart++;
                }

                var border = ws.Cells[1, 1, rowStart - 1, 5].Style.Border;
                border.Left.Style = border.Top.Style = border.Right.Style = border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                ws.Cells["A:AZ"].AutoFitColumns();
                Byte[] bin = pck.GetAsByteArray();
                File.WriteAllBytes(filePath, bin);
            }
        }
        public static void ExportAppToExcel(List<ApplicationData> userApplications, string filePath, string exportUserSID)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("ApplicationData");

                string header = $"Приложения пользователя {exportUserSID} на {DateTime.Now.ToString("dd.MM.yyyy")}";
                ws.Cells["A1"].Value = header;
                ws.Cells["A1"].Style.Font.Size = 20;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Font.Italic = true;
                ws.Cells["A1:C1"].Merge = true;

                ws.Cells["A2"].Value = "Название";
                ws.Cells["B2"].Value = "Вес";
                ws.Cells["C2"].Value = "Дата установки";

                // Устанавливаем стиль шапки таблицы
                using (var range = ws.Cells["A2:C2"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);

                    // Добавляем рамку
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                int rowStart = 3;
                foreach (var item in userApplications)
                {
                    ws.Cells[string.Format("A{0}", rowStart)].Value = item.Name;
                    ws.Cells[string.Format("B{0}", rowStart)].Value = item.Size;
                    ws.Cells[string.Format("C{0}", rowStart)].Value = item.InstallDate.ToString("dd.MM.yyyy");

                    // Добавляем рамку к данным
                    using (var range = ws.Cells[string.Format("A{0}:C{0}", rowStart)])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    rowStart++;
                }

                ws.Cells["A:AZ"].AutoFitColumns();
                Byte[] bin = pck.GetAsByteArray();
                File.WriteAllBytes(filePath, bin);
            }
        }
    }
}
