using System.Diagnostics;

namespace fwp.verbosity
{

    public interface iVerbose
    {

        public enum VerbLevel
        {
            none = 0,
            verbose = 1,
            deep = 2,
        }

        public bool isVerbose(VerbLevel lvl);
        public string stamp();
    }

    /// <summary>
    /// The following color names are supported:
    /// black, blue, green, orange, purple, red, white, and yellow.
    /// </summary>
    [System.Flags]
    public enum VerbositySectionUniversal
    {
        none = 0,
        engine = 1 << 1,
        loading = 1 << 2,

        input = 1 << 3,
        audio = 1 << 4,
        localization = 1 << 5,
        ui = 1 << 6,

        shader = 1 << 7,
        all = ~0,
    }

    public enum VerbosityUnity
    {
        none = 0,
        inputSystem = 1 << 1,
        canvas = 1 << 2,
        addressables = 1 << 3,
        all = ~0,
    }


    /// <summary>
    /// usage :
    /// Verbose.Logger.log
    /// </summary>
    public class Verbose
    {
        static Verbose _logger = null;

        static Verbose()
        {
#if VERBOSITY
			_logger = new();
#endif
        }

        public static Verbose Logger => _logger;

        public const string color_pink_light = "ec3ef2"; // input
        public const string color_green_light = "7df27f"; // flow
        public const string color_red_light = "f23e3e"; // issue
        public const string color_blue_light = "3e83f2"; // app

        /// <summary>
        /// unfiltered log, visible in build
        /// major app event
        /// </summary>
        [Conditional(Verbosity.SYMBOL_VERBOSITY)]
        static public void app(string context, string msg)
            => Verbosity.logCategory("app", context, msg, color_blue_light);

        /// <summary>
        /// unfiltered log, visible in build
        /// major game flow event
        /// </summary>
        [Conditional(Verbosity.SYMBOL_VERBOSITY)]
        static public void flow(string context, string msg)
            => Verbosity.logCategory("flow", context, msg, color_green_light);

        [Conditional(Verbosity.SYMBOL_VERBOSITY)]
        static public void input(string context, string msg)
            => Verbosity.logCategory("input", context, msg, color_pink_light);

        /// <summary>
        /// unfiltered log, visible in build
        /// major game flow event
        /// </summary>
        [Conditional(Verbosity.SYMBOL_VERBOSITY)]
        static public void issue(string context, string msg)
            => Verbosity.logCategory("issue", context, msg, color_red_light);

        /// <summary>
        /// log universal
        /// </summary>
        [Conditional(Verbosity.SYMBOL_VERBOSITY)]
        static public void universal(VerbositySectionUniversal section, string content, object context = null, string hex = null)
            => Verbosity.logFilter(section, content, context, hex);

        [Conditional(Verbosity.SYMBOL_VERBOSITY)]
        static public void unity(VerbosityUnity section, string content, object context = null, string hex = null)
            => Verbosity.logFilter(section, content, context, hex);


    }

}