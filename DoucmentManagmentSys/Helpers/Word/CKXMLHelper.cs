﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using Org.BouncyCastle.Asn1.Cms;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System.Net;
using System.Text.RegularExpressions;

namespace DoucmentManagmentSys.Helpers.Word
{
    public class CKXMLHelper
    {
        public static List<OpenXmlElement> FromCKToXML(string CK)
        {
            //the purpose of this function is to convert from CK HTML to OpenXMl ELements
            var Result = new List<OpenXmlElement>();
            //this function should take the first CKELEMENT only
            var CKTopLevelElement = ExtractFirstTag(CK);
            //id = timestamp
            int timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            
            //now we have parse each tag top-down
            if (WordTemplateHelper.ContainsHtmlTags(CKTopLevelElement, "ul"))
            {

                var newText = WordTemplateHelper.RemoveHtmlTags(CK, "ul");
                //we have to parse each <li> </li>
                foreach (var item in newText.Split("<li>"))
                {
                    if (item != "")
                    {
                        var MidLevelElement = item.Replace("</li>", "");
                        //the result paragraph must be have a bullet point property
                        var LowerLevelElement = ConstructLowLevelElement(MidLevelElement);
                        //create a new paragraph with bullet point property
                        var paragraph = new Paragraph();
                        //create a numbering level
                        ParagraphProperties paragraphProperties = new ParagraphProperties(
                            new ParagraphStyleId() { Val = "ListParagraph" }
                            ,
                            new NumberingProperties(
                                new NumberingLevelReference() { Val = 0 },
                                new NumberingId() { Val = 23 }
                            ),
                            // Set the indentation for the paragraph (e.g., 720 twips for 0.5 inch)
                            new Indentation() { Left = "360" }
                        );
                        paragraph.ParagraphProperties= paragraphProperties;
                        
                        paragraph.Append(LowerLevelElement);
                        Result.Add(paragraph);
                    }

                }

            }
            else if(WordTemplateHelper.ContainsHtmlTags(CKTopLevelElement, "p"))
            {
                var MidLevelElement = WordTemplateHelper.RemoveHtmlTags(CKTopLevelElement, "p");
                var LowerLevelElement = ConstructLowLevelElement(MidLevelElement);
                Result.AddRange(LowerLevelElement);

            }
            else
            {
                throw new Exception("Unconvertable CK Element:" + CKTopLevelElement);
            }
            return Result;
        }
        public static string ExtractFirstTag(string HTML)
        {
            var match = Regex.Match(HTML, @"<(.*?)>(.*?)<\/\1>", RegexOptions.Singleline);
            return match.Success ? match.Value : HTML;
        }
        public static List<Run> ConstructLowLevelElement(string HTMLs)
        {
            //we have to check if there is a strong tag then the Run must have a bold property
            //if there is i tag then the Run must have an italics property
            var RunsResults = new List<Run>();
            foreach (var span in WordTemplateHelper.SplitByTag(HTMLs,"span"))
            {
                if (span=="")
                {
                    continue;
                }

                var HTML = span;


                var run = new Run();
                var runProperties = new RunProperties();
                if (WordTemplateHelper.HasAttributeWithValue(HTML, "contenteditable", "false"))
                {
                    //add darkMagenta HighLight
                    runProperties.Append(new Highlight() { Val = HighlightColorValues.DarkMagenta });
                }
                if (WordTemplateHelper.HasAttributeWithValue(HTML, "variable"))
                {
                    //add darkMagenta HighLight
                    OpenXmlAttribute openXmlAttribute = new OpenXmlAttribute("Variable", "http://DMSNamespace", WordTemplateHelper.GetAttributeValue(HTML, "Variable"));
                    run.SetAttribute(openXmlAttribute);
                }
                if (WordTemplateHelper.TagStyleContainsAttribute(HTML, "font-weight", "bold"))
                {
                     // Create new run properties
                    var bold = new Bold(); // Create a new bold property

                    runProperties.Append(bold); // Add the bold property to the run properties
                
                    
                }
                if (WordTemplateHelper.TagStyleContainsAttribute(HTML, "font-style", "italic"))
                {
                    // Create new run properties
                    var italics = new Italic(); // Create a new italics property

                    runProperties.Append(italics); // Add the italics property to the run properties
                
                    
                
                }
              

                
                string startSeq = "font-size:";
                string endSeq = "px";

                string pattern = $"{Regex.Escape(startSeq)}(.*?){Regex.Escape(endSeq)}";

                Match match = Regex.Match(HTML, pattern);
                string extractedString = "28";
                if (match.Success)
                {
                    extractedString = match.Groups[1].Value;
                    int adjustedFontSize = int.Parse(extractedString) + 12;
                    extractedString = adjustedFontSize.ToString();
                        
                }


                
                runProperties.FontSize = new FontSize() { Val = extractedString };
                HTML = WordTemplateHelper.RemoveHtmlTags(HTML, "span");
                run.Append(runProperties);
                run.Append(new Text(WebUtility.HtmlDecode(HTML)));
                RunsResults.Add(run);
            }

            
            return RunsResults;
        }
    }
}
