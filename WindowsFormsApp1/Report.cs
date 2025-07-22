using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

public class OpenOfficeTableCreator
{
    public void CreateDictionaryTable(Dictionary<string, string> dictionary, string filePath)
    {
        // Создаем новый Word-документ
        using (WordprocessingDocument wordDocument =
            WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
        {
            // Добавляем основную часть документа
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            // Создаем таблицу
            Table table = new Table();

            // Настраиваем границы таблицы
            TableProperties tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 12 },
                    new BottomBorder { Val = BorderValues.Single, Size = 12 },
                    new LeftBorder { Val = BorderValues.Single, Size = 12 },
                    new RightBorder { Val = BorderValues.Single, Size = 12 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 12 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 12 }
                ),
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
            );

            table.AppendChild(tableProperties);

            // Создаем строку заголовков
            TableRow headerRow = new TableRow();
            headerRow.Append(
                CreateCell("Компонент", true),
                CreateCell("Количество", true)
            );
            table.Append(headerRow);

            // Добавляем данные из словаря
            foreach (KeyValuePair<string, string> entry in dictionary)
            {
                TableRow dataRow = new TableRow();
                dataRow.Append(
                    CreateCell(entry.Key),
                    CreateCell(entry.Value)
                );
                table.Append(dataRow);
            }

            // Добавляем таблицу в документ
            body.Append(table);
            mainPart.Document.Save();
        }
    }

    private TableCell CreateCell(string text, bool isHeader = false)
    {
        TableCell cell = new TableCell();
        Paragraph paragraph = new Paragraph();
        Run run = new Run();

        // Настройки форматирования текста
        RunProperties runProperties = new RunProperties();
        if (isHeader)
        {
            runProperties.Append(new Bold());
            runProperties.Append(new FontSize { Val = "24" });
        }
        else
        {
            runProperties.Append(new FontSize { Val = "20" });
        }

        run.Append(runProperties);
        run.Append(new Text(text));
        paragraph.Append(run);
        cell.Append(paragraph);

        // Настройки ячейки
        TableCellProperties cellProperties = new TableCellProperties(
            new TableCellWidth { Type = TableWidthUnitValues.Auto }
        );
        cell.Append(cellProperties);

        return cell;
    }

}