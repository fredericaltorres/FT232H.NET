using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fLogViewer.PlugIns.FileProviders.BinaryFileProvider
{

    public class BinaryToTextGenerator //: LogViewer.Net.FileProviderSuperBaseClass
    {
        public string _fileName { get; }

        private List<byte> mainBuffer;

        public BinaryToTextGenerator(string fileName, string text = null)
        {
            if(fileName != null)
            {
                this._fileName = fileName;
                this.mainBuffer = LoadFile(this._fileName);
            }
            else
            {
                this.mainBuffer = Encoding.UTF8.GetBytes(text).ToList();
            }
            
            _fileName = fileName;
        }

        public string Generate(BinaryViewerOption options)
        {
            var sb = new StringBuilder(mainBuffer.Count * 2);
            var bytePerLineCounter = 0;
            var totalSizeProcessed = 0;
            var hexaRep = new StringBuilder(1024);
            var decRep = new StringBuilder(1024);
            var asciRep = new StringBuilder(1024);

            if (_fileName != null)
            {
                var fi = new FileInfo(_fileName);
                sb.Append($"File: {_fileName}").AppendLine();
                sb.Append($"Size:{fi.Length}, LastModfied:{fi.LastWriteTime}, CreationDate:{fi.CreationTime}, Attributes:{fi.Attributes}").AppendLine().AppendLine();
            }

            var sectorIndex = 0;

            while (true)
            {
                asciRep.Clear();
                hexaRep.Clear();
                decRep.Clear();
                var tmpBuffer = mainBuffer.Skip(bytePerLineCounter * options.bytePerLine).Take(options.bytePerLine).ToList();
                if (tmpBuffer.Count == 0)
                    break;

                if (options.ShowSector && totalSizeProcessed % options.SectorSize == 0)
                {
                    sb.AppendLine().Append($"Sector {sectorIndex} - {sectorIndex * options.SectorSize} ").AppendLine();
                    sectorIndex += 1;
                }


                if (options.ShowAscii)
                {
                    foreach (byte b in tmpBuffer)
                    {
                        var bb = b;
                        if ((b < 32) || (b >= 128))
                        {
                            bb = (byte)'.';
                        }
                        asciRep.AppendFormat("{0}", (char)bb);
                    }
                }


                if (options.ShowHexaDecimal)
                {
                    if (options.ShowBinary)
                    {
                        foreach (byte b in tmpBuffer)
                        {
                            var bv = Convert.ToString(b, 2);
                            if (b == 0)
                            {
                                bv = "".PadLeft(8, ' ');
                                hexaRep.AppendFormat("{0:X2} {1} ", b, bv);
                            }
                            else
                            {
                                bv = bv.PadLeft(8, '0');
                                hexaRep.AppendFormat("{0:X2}:{1} ", b, bv);
                            }

                        }
                    }
                    else
                    {
                        foreach (byte b in tmpBuffer)
                            hexaRep.AppendFormat("{0:X2} ", b);
                        var len = hexaRep.Length;
                        if(len % options.bytePerLine != 0)
                        {
                            hexaRep.Append("".PadLeft((options.bytePerLine * 3)-len));
                            asciRep.Append("".PadLeft((options.bytePerLine * 1) - asciRep.Length));
                        }
                    }
                    hexaRep = hexaRep.Remove(hexaRep.Length - 1, 1); // Remove last space

                }
                if (options.ShowDecimal)
                {
                    foreach (byte b in tmpBuffer)
                        decRep.AppendFormat("{0:000} ", b);
                }

                var sepaString = $" {options.VerticalBar} ";
                sb.AppendFormat("{0:000000000}", bytePerLineCounter);
                sb.Append(sepaString);

                if (options.ShowHexaDecimal)
                {
                    sb.Append(hexaRep);
                    sb.Append(sepaString);
                }

                if (options.ShowDecimal)
                {
                    sb.Append(decRep);
                    sb.Append(sepaString);
                }

                if (options.ShowAscii)
                {
                    sb.Append(asciRep);
                    sb.Append(sepaString);
                }

                sb.AppendLine();

                bytePerLineCounter += 1;
                totalSizeProcessed = bytePerLineCounter * options.bytePerLine;
            }
            sb.AppendLine();
            if (options.GenerateCArrayCode)
            {
                GenerateCArrayCode(mainBuffer, sb, options, this._fileName);
            }
            return sb.ToString();
        }

        private List<byte> LoadFile(string fileName)
        {
            byte[] buffer = null;
            
            using (FileStream fs = File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                int len = Convert.ToInt32(fs.Length);
                buffer = new byte[len];
                fs.Read(buffer, 0, len);
            }

            return buffer.ToList();
        }

        private static void GenerateCArrayCode(List<byte> buffer2, StringBuilder sb, BinaryViewerOption options, string fileName)
        {
            var bytePerLineCounter = 0;

            sb.AppendFormat($"// C Array filename: {fileName}").AppendLine();
            sb.Append($"uint8_t c_array[ARRAY_SIZE] = {{ // {buffer2.Count} bytes, {buffer2.Count/1024} Kb ").AppendLine();
            while (true)
            {
                var buffer = buffer2.Skip(bytePerLineCounter * options.bytePerLine).Take(options.bytePerLine).ToList();
                if (buffer.Count == 0)
                    break;

                sb.AppendFormat("    ");
                foreach (byte b in buffer)
                    sb.AppendFormat("0x{0:X2}, ", b);

                sb.Append($" // {bytePerLineCounter:000000}");
                sb.AppendLine();
                bytePerLineCounter += 1;
            }
            sb.AppendFormat("}};").AppendLine();
        }
    }
}

