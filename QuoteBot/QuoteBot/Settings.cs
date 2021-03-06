﻿using System;
using System.IO;
using System.Xml.Serialization;

namespace TwitchBot {
    public class Settings {
        [XmlElement("account")]
        public string AccountFile { get; set; }

        [XmlElement("channel")]
        public string Channel { get; set; }

        [XmlElement("quotes")]
        public string QuotesFile { get; set; }

        [XmlElement("greeting")]
        public string Greeting { get; set; }

        [XmlElement("interval")]
        public int Interval { get; set; }

        [XmlElement("color")]
        public string Color { get; set; }

        public Settings() {
            AccountFile = "%userprofile%/account";
            Channel = "";

            QuotesFile = "%userprofile%/quotes.txt";

            Greeting = "";

            Interval = 1000;

            Color = "green";

            ExpandEnvironmentVariables();
        }

        public static Settings Load(string fileName) {
            Settings settings = new Settings();

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            
            Stream stream = File.OpenRead(fileName);
            settings = (Settings)serializer.Deserialize(stream);
            stream.Close();

            File.Delete(fileName);

            stream = File.OpenWrite(fileName);
            serializer.Serialize(stream, settings);
            stream.Close();

            settings.ExpandEnvironmentVariables();

            return settings;
        }

        private void ExpandEnvironmentVariables() {
            AccountFile = Environment.ExpandEnvironmentVariables(AccountFile);
            QuotesFile = Environment.ExpandEnvironmentVariables(QuotesFile);
        }
    }
}
