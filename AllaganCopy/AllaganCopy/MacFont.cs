using System;
using System.Collections.Generic;
using System.IO;

namespace AllaganCopy
{
    class MacFont
    {
        // produced modified global index file that points to korean font texture.
        public static void ProduceMacFont(string globalPath, string koreanPath, string releasePath)
        {
            // get global data file paths.
            string globalIndexPath = Path.GetFullPath(Path.Combine(globalPath, "sqpack", "ffxiv", "000000.win32.index"));
            string globalDatPath = Path.GetFullPath(Path.Combine(globalPath, "sqpack", "ffxiv", "000000.win32.dat0"));

            // get korean data file paths.
            string koIndexPath = Path.GetFullPath(Path.Combine(koreanPath, "sqpack", "ffxiv", "000000.win32.index"));
            string koDatPath = Path.GetFullPath(Path.Combine(koreanPath, "sqpack", "ffxiv", "000000.win32.dat0"));

            // copy original global data files to output paths.
            string outputIndexPath = Path.GetFullPath(Path.Combine(releasePath, Path.GetFileName(globalIndexPath)));
            File.Copy(globalIndexPath, outputIndexPath, true);

            string outputDatPath = Path.GetFullPath(Path.Combine(releasePath, Path.GetFileName(globalDatPath)));
            File.Copy(globalDatPath, outputDatPath, true);

            // copy korean data file and rename it as dat1.
            string outputNewDatPath = Path.GetFullPath(Path.Combine(releasePath, "000000.win32.dat1"));
            File.Copy(koDatPath, outputNewDatPath, true);

            // set up which indices will point to korean data file.
            // key is the global key and value is the korean key.
            Dictionary<uint, uint> exchanges = new Dictionary<uint, uint>();

            // main fonts.
            exchanges.Add(Hash.Compute("font1.tex"), Hash.Compute("font_krn_1.tex"));
            exchanges.Add(Hash.Compute("font2.tex"), Hash.Compute("font_krn_2.tex"));
            exchanges.Add(Hash.Compute("font3.tex"), Hash.Compute("font_krn_3.tex"));

            // axis fonts.
            /*exchanges.Add(Hash.Compute("axis_12.fdt"), Hash.Compute("KrnAxis_120.fdt"));
            exchanges.Add(Hash.Compute("axis_14.fdt"), Hash.Compute("KrnAxis_140.fdt"));
            exchanges.Add(Hash.Compute("axis_18.fdt"), Hash.Compute("KrnAxis_180.fdt"));*/

            // pre-hash the key for common/font directory since it'll be frequently used.
            uint fontDirectoryKey = Hash.Compute("common/font");

            // let's process korean index file first, read offsets and cache them.
            Dictionary<uint, uint> koreanOffsets = new Dictionary<uint, uint>();

            // add all wanted korean keys to dictionary first for fast searching later.
            foreach (uint key in exchanges.Keys)
            {
                koreanOffsets.Add(exchanges[key], 0);
            }

            // read korean index file as bytes.
            byte[] index = File.ReadAllBytes(koIndexPath);

            int headerOffset = BitConverter.ToInt32(index, 0xc);
            int fileOffset = BitConverter.ToInt32(index, headerOffset + 0x8);
            int fileCount = BitConverter.ToInt32(index, headerOffset + 0xc) / 0x10;

            // go through each file in korean index file and see if our target key is there.
            for (int i = 0; i < fileCount; i++)
            {
                int keyOffset = fileOffset + i * 0x10;

                // if the directory is not common/font, skip.
                if (BitConverter.ToUInt32(index, keyOffset + 0x4) != fontDirectoryKey) continue;

                uint key = BitConverter.ToUInt32(index, keyOffset);

                // if target key is found, save the offset.
                if (koreanOffsets.ContainsKey(key))
                {
                    uint offset = BitConverter.ToUInt32(index, keyOffset + 0x8);

                    // set last 3 bytes to 0x2 to indicate that this index will point to dat1 file.
                    offset = (offset & 0xfffffff8) | 0x2;

                    koreanOffsets[key] = offset;
                }
            }

            // read output index file as bytes.
            index = File.ReadAllBytes(outputIndexPath);
            
            headerOffset = BitConverter.ToInt32(index, 0xc);

            // set available dat file numbers to 2 to include dat0 and dat1.
            index[headerOffset + 0x50] = 2;

            fileOffset = BitConverter.ToInt32(index, headerOffset + 0x8);
            fileCount = BitConverter.ToInt32(index, headerOffset + 0xc) / 0x10;

            // go through each file in the index file.
            for (int i = 0; i < fileCount; i++)
            {
                int keyOffset = fileOffset + i * 0x10;

                // if the directory is not common/font, skip.
                if (BitConverter.ToUInt32(index, keyOffset + 0x4) != fontDirectoryKey) continue;

                uint key = BitConverter.ToUInt32(index, keyOffset);

                // if target key is wanted for swapping, swap the offset written in the bytes to korean file offset.
                if (exchanges.ContainsKey(key))
                {
                    if (koreanOffsets[exchanges[key]] == 0) throw new Exception();
                    Array.Copy(BitConverter.GetBytes(koreanOffsets[exchanges[key]]), 0, index, keyOffset + 0x8, 0x4);
                }
            }

            // write the processed bytes back to output index file.
            File.WriteAllBytes(outputIndexPath, index);
        }
    }
}
