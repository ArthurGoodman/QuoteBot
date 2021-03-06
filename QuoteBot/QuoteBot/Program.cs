﻿using System;
using System.IO;

namespace TwitchBot {
    public class Program {
        static void Main(string[] args) {
            try {
                Bot bot = new Bot();
                bot.Run();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine("<press any key to continue>");
                File.WriteAllText("error.log", e.ToString());
                Console.ReadKey();
                return;
            }
        }
    }
}
