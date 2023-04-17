using libVT100;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class HtmlAnsiDecoder : IAnsiDecoderClient
    {
        public HtmlAnsiDecoder(PipeWriter pipeWriter)
        {
            this.pipeWriter = pipeWriter;
        }

        void WriteByte(byte b)
        {
            var result = pipeWriter.WriteAsync(new byte[] { b }).Result;
            if ((b & 128) == 0) { result = pipeWriter.FlushAsync().Result; }
        }
        void WriteString(string str)
        {
            var written = Encoding.UTF8.GetBytes(str, pipeWriter);
            //pipeWriter.Advance((int)written);
            var result = pipeWriter.FlushAsync().Result;
        }
        bool fillspaces = true;
        public void Bytes(IAnsiDecoder _sender, byte[] _bytes)
        {
            foreach (var b in _bytes)
            {
                char c = (char)b;
                if (c == ' ' && fillspaces) { WriteString(" &nbsp;"); }
                else
                {
                    fillspaces = false;
                    WriteByte(b);
                }
                if (c == '\n') { WriteString("<br>"); fillspaces = true; }
            }
        }
        public void Characters(IAnsiDecoder _sender, char[] _chars)
        {
            foreach (var c in _chars)
            {
                if (c == ' ' && fillspaces) { WriteString(" &nbsp;"); }
                else
                {
                    fillspaces = false;
                    WriteString(c.ToString());
                }
                if (c == '\n') { WriteString("<br>"); fillspaces = true; }
            }
        }

        public void ClearLine(IAnsiDecoder _sender, ClearDirection _direction)
        {
            throw new NotImplementedException();
        }

        public void ClearScreen(IAnsiDecoder _sender, ClearDirection _direction)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Point GetCursorPosition(IAnsiDecoder _sender)
        {
            throw new NotImplementedException();
        }

        public Size GetSize(IAnsiDecoder _sender)
        {
            throw new NotImplementedException();
        }

        public void ModeChanged(IAnsiDecoder _sender, AnsiMode _mode)
        {
            throw new NotImplementedException();
        }

        public void MoveCursor(IAnsiDecoder _sender, Direction _direction, int _amount)
        {
            throw new NotImplementedException();
        }

        public void MoveCursorTo(IAnsiDecoder _sender, Point _position)
        {
            throw new NotImplementedException();
        }

        public void MoveCursorToBeginningOfLineAbove(IAnsiDecoder _sender, int _lineNumberRelativeToCurrentLine)
        {
            throw new NotImplementedException();
        }

        public void MoveCursorToBeginningOfLineBelow(IAnsiDecoder _sender, int _lineNumberRelativeToCurrentLine)
        {
            throw new NotImplementedException();
        }

        public void MoveCursorToColumn(IAnsiDecoder _sender, int _columnNumber)
        {
            throw new NotImplementedException();
        }

        public void RestoreCursor(IAnsiDecoder _sender)
        {
            throw new NotImplementedException();
        }

        public void SaveCursor(IAnsiDecoder _sernder)
        {
            throw new NotImplementedException();
        }

        public void ScrollPageDownwards(IAnsiDecoder _sender, int _linesToScroll)
        {
            throw new NotImplementedException();
        }

        public void ScrollPageUpwards(IAnsiDecoder _sender, int _linesToScroll)
        {
            throw new NotImplementedException();
        }

        bool bold;
        bool span;
        bool underline;

        // https://chrisyeh96.github.io/2020/03/28/terminal-colors.html
        Dictionary<GraphicRendition, string> colormap = new Dictionary<GraphicRendition, string> {
            { GraphicRendition.ForegroundBrightBlack, "#555753" },
            { GraphicRendition.ForegroundNormalCyan, "#06989a" },
            { GraphicRendition.ForegroundNormalGreen, "#4e9a06" },
            { GraphicRendition.ForegroundNormalYellow, "#c4a000" },
            { GraphicRendition.ForegroundNormalRed, "#cc0000" }
        };
        private readonly PipeWriter pipeWriter;

        public void SetGraphicRendition(IAnsiDecoder _sender, GraphicRendition[] _commands)
        {
            foreach (GraphicRendition command in _commands)
            {
                switch (command)
                {
                    case GraphicRendition.Underline:
                        if (!underline)
                        {
                            WriteString("<u>");
                        }
                        underline = true;
                        break;
                    case GraphicRendition.Bold:
                        if (!bold)
                        {
                            WriteString("<b>");
                        }
                        bold = true;
                        break;
                    case GraphicRendition.Reset:
                        if (underline)
                        {
                            WriteString("</u>");
                        }
                        if (bold)
                        {
                            WriteString("</b>");
                        }
                        if (span)
                        {
                            WriteString("</span>");
                        }
                        bold = false;
                        span = false;
                        break;
                    case GraphicRendition.ForegroundBrightBlack:
                    case GraphicRendition.ForegroundNormalGreen:
                    case GraphicRendition.ForegroundNormalCyan:
                    case GraphicRendition.ForegroundNormalYellow:
                    case GraphicRendition.ForegroundNormalRed:
                        if (span)
                        {
                            WriteString("</span>");
                        }
                        WriteString($"<span style='color:{colormap[command]}'>");
                        span = true;
                        break;
                    default:
                        WriteString($"*{command.ToString()}*");
                        break;
                }   
            }
        }
    }
}
