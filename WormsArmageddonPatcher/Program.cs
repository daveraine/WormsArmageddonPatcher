using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WormsArmageddonPatcher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var version = typeof(Program).Assembly.GetName().Version.ToString();
            Console.WriteLine($"Worms Armageddon Team File Patcher v{version}");
            Console.WriteLine();

            if (args.Length == 0)
            {
                Exit("Path to WGT file must be specified.", -1);
                return;
            }

            if (args.Length > 1)
            {
                Exit("Only one file must be specified.", -1);
                return;
            }

            var magicNumber = GetSignature(new byte[] { 0x57, 0x47, 0x54, 0x00 });
            var path = args[0];

            if (!File.Exists(path))
            {
                Exit($"File {path} does not exist!", -1);
                return;
            }

            using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            using var reader = new BinaryReader(fs);
            var signatureBytes = reader.ReadBytes(4);
            var signature = GetSignature(signatureBytes);

            if (signature != magicNumber)
            {
                Exit("Not a Worms Armageddon team file!", -1);
                return;
            }

            fs.Position = 0x06;
            var utilityUpgrades = (UtilityUpgrade)fs.ReadByte();
            var weaponUpgrades = (WeaponUpgrade)fs.ReadByte();
            var cheats = (Cheat)fs.ReadByte();
            int fullWormage = fs.ReadByte();

            Console.WriteLine("Utility Upgrades:");
            DisplayFlags(utilityUpgrades);

            Console.WriteLine();
            Console.WriteLine("Weapon Upgrades:");
            DisplayFlags(weaponUpgrades);

            Console.WriteLine();
            Console.WriteLine("Cheats:");
            DisplayFlags(cheats);

            Console.WriteLine();
            Console.WriteLine("Indestructible terrain and Full Wormage scheme:");
            Console.WriteLine(fullWormage == 0 ? "No" : "Yes");

            Console.WriteLine();

            do
            {
                Console.Write("Patch file for full unlock? (Y/N): ");
                var answer = Console.ReadKey();
                Console.WriteLine();

                if (answer.Key == ConsoleKey.Y)
                {
                    var allUtilityUpgrades = UtilityUpgrade.LaserSight | UtilityUpgrade.FastWalk | UtilityUpgrade.Invisibility | UtilityUpgrade.LowGravity | UtilityUpgrade.JetPack;
                    WriteFlags(fs, 0x06, allUtilityUpgrades);

                    var allWeaponUpgrades = WeaponUpgrade.Grenade | WeaponUpgrade.Shotgun | WeaponUpgrade.BananaBomb | WeaponUpgrade.Longbox | WeaponUpgrade.AquaSheep;
                    WriteFlags(fs, 0x07, allWeaponUpgrades);

                    var allCheats = Cheat.GodWorms | Cheat.Blood | Cheat.SheepHeaven;
                    WriteFlags(fs, 0x08, allCheats);

                    fs.Position = 0x09;
                    fs.WriteByte(Convert.ToByte(1));

                    fs.Flush();

                    Exit("File patched.");
                    break;
                }
                else if (answer.Key == ConsoleKey.N)
                {
                    Exit("File not patched.");
                    break;
                }
            }
            while (true);
        }

        private static void Exit(string message, int exitCode = 0)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(exitCode);
        }

        private static int GetSignature(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }

        private static void DisplayFlags<TEnum>(TEnum input)
            where TEnum : Enum
        {
            var flags = GetFlags(input);
            if (flags.Any())
            {
                foreach (var flag in flags)
                {
                    Console.WriteLine(Enum.GetName(typeof(TEnum), flag));
                }
            }
            else
            {
                Console.WriteLine("None");
            }
        }

        private static IEnumerable<TEnum> GetFlags<TEnum>(TEnum input)
            where TEnum : Enum
        {
            var zeroFlag = Enum.Parse(input.GetType(), "0");

            foreach (Enum value in Enum.GetValues(input.GetType()))
                if (!zeroFlag.Equals(value) && input.HasFlag(value))
                    yield return (TEnum)value;
        }

        private static void WriteFlags<TEnum>(FileStream fileStream, int position, TEnum input)
            where TEnum : Enum
        {
            fileStream.Position = position;
            fileStream.WriteByte(Convert.ToByte(input));
        }

        [Flags]
        enum UtilityUpgrade
        {
            None = 0,
            LaserSight = 1,
            FastWalk = 2,
            Invisibility = 4,
            LowGravity = 8,
            JetPack = 16
        }

        [Flags]
        enum WeaponUpgrade
        {
            None = 0,
            Grenade = 1,
            Shotgun = 2,
            BananaBomb = 4,
            Longbox = 8,
            AquaSheep = 16
        }

        [Flags]
        enum Cheat
        {
            None = 0,
            GodWorms = 1,
            Blood = 2,
            SheepHeaven = 4
        }
    }
}
