﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using W3Edit.W3Strings;

namespace W3Edit.W3Speech
{
    public class W3SoundInfo
    {
        public uint id;
        public uint id_high;
        public uint wave_offs;
        public uint wave_size;
        public uint cr2w_offs;
        public uint cr2w_size;
        public byte[] Wave_File;
        public byte[] CR2W_File;
    }
    public class W3Speech
    {
        public static readonly byte[] IDString = {(byte) 'C', (byte) 'P', (byte) 'S', (byte) 'W'};
        public static readonly byte[] Version = {0xA2, 0x00, 0x00, 0x00};

        public void Read(BinaryReader br)
        {
            var magic = br.ReadChars(4); //Has to be cpsw
            if (magic.SequenceEqual(new char[] { 'C', 'P', 'S', 'Q' }))
                throw new Exception("Not a valid w3speech file!");
            Console.WriteLine("Magic: " + new string(magic));
            var version = br.ReadUInt16(); //Should be 162
            Console.WriteLine("Version: " + version);
            var key1 = br.ReadUInt16();
            Console.WriteLine("Key: " + key1.ToString("X"));
            var soundInfoList = new List<W3SoundInfo>();
            var count = br.ReadBit6();
            for (var i = 0; i < count; i++)
            {
                var sound = new W3SoundInfo
                {
                    id = br.ReadUInt32(),
                    id_high = br.ReadUInt32(),
                    wave_offs = br.ReadUInt32(),
                    wave_size = br.ReadUInt32(),
                    cr2w_offs = br.ReadUInt32(),
                    cr2w_size = br.ReadUInt32()
                };
                soundInfoList.Add(sound);
            }
            var key2 = br.ReadUInt16();
            key1 = (ushort)(key1 << 16 | key2);
            Console.WriteLine("Key: " + key1.ToString("X"));
            var magic_n_lang = W3LanguageKey.Get(key1);
            Console.WriteLine("Magic: " + magic_n_lang.Key + " Language: " + magic_n_lang.Language);
            soundInfoList.Select(x => x.id = ~magic_n_lang.Key);
            soundInfoList.OrderBy(x => x.id);
            Console.WriteLine("Sorting sound entries...");
            var fileInfo = new List<string>();
            soundInfoList.ForEach(sound => fileInfo.Add("Id: " + sound.id + " Id_high: " + sound.id_high + " Wawe offset: " + sound.wave_offs + " Wawe size: " + sound.wave_size + " CR2W Size: " + sound.cr2w_size + " CR2W Offset: " + sound.cr2w_offs));
            File.WriteAllLines("log.txt", fileInfo);
            foreach (var t in soundInfoList)
            {
                if (t.wave_size > 0)
                {
                    br.BaseStream.Seek(t.wave_offs, SeekOrigin.Begin);
                    t.Wave_File = br.ReadBytes((int)t.wave_size);
                }
                if (t.cr2w_size > 0)
                {
                    br.BaseStream.Seek(t.cr2w_offs, SeekOrigin.Begin);
                    t.CR2W_File = br.ReadBytes((int)t.cr2w_size);
                }
            }
            Console.WriteLine("Wave file count: " + soundInfoList.Count(x => x.wave_size > 0));
            Console.WriteLine("CR2W file count: " + soundInfoList.Count(x => x.cr2w_size > 0));
        }

        public void Write(BinaryWriter bw)
        {
        }
    }
}