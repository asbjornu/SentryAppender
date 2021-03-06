﻿using System;

using SharpRaven.Data;
using SharpRaven.Log4Net.Extra;

using log4net.Appender;
using log4net.Core;

namespace SharpRaven.Log4Net
{
    public class SentryAppender : AppenderSkeleton
    {
        private static RavenClient ravenClient;
        public string DSN { get; set; }
        public string Logger { get; set; }


        protected override void Append(LoggingEvent loggingEvent)
        {
            if (ravenClient == null)
            {
                ravenClient = new RavenClient(DSN)
                {
                    Logger = Logger
                };
            }

            object extra = new
            {
                Environment = new EnvironmentExtra(),
                Http = new HttpExtra(),
            };

            var exception = loggingEvent.ExceptionObject ?? loggingEvent.MessageObject as Exception;

            if (exception != null)
            {
                ravenClient.CaptureException(exception, null, extra);
            }
            else
            {
                var level = Translate(loggingEvent.Level);
                var message = loggingEvent.RenderedMessage;

                if (message != null)
                {
                    ravenClient.CaptureMessage(message, level, null, extra);
                }
            }
        }


        internal static ErrorLevel Translate(Level level)
        {
            switch (level.DisplayName)
            {
                case "WARN":
                    return ErrorLevel.warning;

                case "NOTICE":
                    return ErrorLevel.info;
            }

            ErrorLevel errorLevel;

            return !Enum.TryParse(level.DisplayName, true, out errorLevel)
                       ? ErrorLevel.error
                       : errorLevel;
        }


        protected override void Append(LoggingEvent[] loggingEvents)
        {
            foreach (var loggingEvent in loggingEvents)
            {
                Append(loggingEvent);
            }
        }
    }
}