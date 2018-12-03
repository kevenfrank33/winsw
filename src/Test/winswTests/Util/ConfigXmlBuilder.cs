﻿using System;
using System.Collections.Generic;
using System.Text;
using winsw;
using winsw.Plugins.RunawayProcessKiller;
using winswTests.Extensions;

namespace winswTests.Util
{
    /// <summary>
    /// Configuration XML builder, which simplifies testing of WinSW Configuration file.
    /// </summary>
    class ConfigXmlBuilder
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string Executable { get; set; }
        public bool PrintXMLVersion { get; set; }
        public string XMLComment { get; set; }
        public List<string> ExtensionXmls { get; private set; }

        private readonly List<string> configEntries;

        // TODO: Switch to the initializer?
        private ConfigXmlBuilder()
        {
            configEntries = new List<string>();
            ExtensionXmls = new List<string>();
        }

        public static ConfigXmlBuilder create(string id = null, string name = null,
            string description = null, string executable = null, bool printXMLVersion = true,
            string xmlComment = "")
        {
            var config = new ConfigXmlBuilder();
            config.Id = id ?? "myapp";
            config.Name = name ?? "MyApp Service";
            config.Description = description ?? "MyApp Service (powered by WinSW)";
            config.Executable = executable ?? "%BASE%\\myExecutable.exe";
            config.PrintXMLVersion = printXMLVersion;
            config.XMLComment = (xmlComment != null && xmlComment.Length == 0)
                ? "Just a sample configuration file generated by the test suite"
                : xmlComment;
            return config;
        }

        public string ToXMLString(bool dumpConfig = false)
        {
            StringBuilder str = new StringBuilder();
            if (PrintXMLVersion)
            {
                // TODO: The encoding is generally wrong
                str.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            }

            if (XMLComment != null)
            {
                str.AppendFormat("<!--{0}-->\n", XMLComment);
            }

            str.Append("<service>\n");
            str.AppendFormat("  <id>{0}</id>\n", Id);
            str.AppendFormat("  <name>{0}</name>\n", Name);
            str.AppendFormat("  <description>{0}</description>\n", Description);
            str.AppendFormat("  <executable>{0}</executable>\n", Executable);
            foreach (string entry in configEntries)
            {
                // We do not care much about pretty formatting here
                str.AppendFormat("  {0}\n", entry);
            }

            // Extensions
            if (ExtensionXmls.Count > 0)
            {
                str.Append("  <extensions>\n");
                foreach (string xml in ExtensionXmls)
                {
                    str.Append(xml);
                }

                str.Append("  </extensions>\n");
            }

            str.Append("</service>\n");
            string res = str.ToString();
            if (dumpConfig)
            {
                Console.Out.WriteLine("Produced config:");
                Console.Out.WriteLine(res);
            }

            return res;
        }

        public ServiceDescriptor ToServiceDescriptor(bool dumpConfig = false)
        {
            return ServiceDescriptor.FromXML(ToXMLString(dumpConfig));
        }

        public ConfigXmlBuilder WithRawEntry(string entry)
        {
            configEntries.Add(entry);
            return this;
        }

        public ConfigXmlBuilder WithTag(string tagName, string value)
        {
            return WithRawEntry(string.Format("<{0}>{1}</{0}>", tagName, value));
        }

        public ConfigXmlBuilder WithRunawayProcessKiller(RunawayProcessKillerExtension ext, string extensionId = "killRunawayProcess", bool enabled = true)
        {
            var fullyQualifiedExtensionName = ExtensionTestBase.GetExtensionClassNameWithAssembly(typeof(RunawayProcessKillerExtension));
            StringBuilder str = new StringBuilder();
            str.AppendFormat("    <extension enabled=\"{0}\" className=\"{1}\" id=\"{2}\">\n", new object[] { enabled, fullyQualifiedExtensionName, extensionId });
            str.AppendFormat("      <pidfile>{0}</pidfile>\n", ext.Pidfile);
            str.AppendFormat("      <stopTimeout>{0}</stopTimeout>\n", ext.StopTimeout.TotalMilliseconds);
            str.AppendFormat("      <stopParentFirst>{0}</stopParentFirst>\n", ext.StopParentProcessFirst);
            str.AppendFormat("      <checkWinSWEnvironmentVariable>{0}</checkWinSWEnvironmentVariable>\n", ext.CheckWinSWEnvironmentVariable);
            str.Append("    </extension>\n");
            ExtensionXmls.Add(str.ToString());

            return this;
        }

        public ConfigXmlBuilder WithDownload(Download download)
        {
            StringBuilder str = new StringBuilder();
            str.AppendFormat("<download from=\"{0}\" to=\"{1}\" failOnError=\"{2}\"", new object[] { download.From, download.To, download.FailOnError });

            // Authentication
            if (download.Auth != Download.AuthType.none)
            {
                str.AppendFormat(" auth=\"{0}\"", download.Auth);
                if (download.Auth == Download.AuthType.basic)
                {
                    str.AppendFormat(" user=\"{0}\" password=\"{1}\"", new object[] { download.Username, download.Password });
                }

                if (download.UnsecureAuth)
                {
                    str.AppendFormat(" unsecureAuth=\"true\"");
                }
            }

            str.Append("/>");

            return WithRawEntry(str.ToString());
        }

        public ConfigXmlBuilder WithDelayedAutoStart()
        {
            return WithRawEntry("<delayedAutoStart/>");
        }
    }
}
