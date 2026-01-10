/// <summary>
/// Naming convention: ITG_ = Item, EVS_ = Event, FCP_ = Field
/// </summary>
public static class CardIds
{
    // =========================================================================
    // ITEM CARDS (ITG_)
    // =========================================================================
    public static class Items
    {
        public const string HUND = "ITG001";
        public const string ZEUGNIS = "ITG002";
        public const string SPORTWAGEN = "ITG003";
        public const string ESCOOTER = "ITG006";
        public const string DIEBESGUT = "ITG008";
        public const string RABATTMARKERL = "ITG014";
        public const string HANDSCHELLEN = "ITG016";
        public const string ALKOHOL = "ITG018";
        public const string BASKENMUETZE = "ITG025";
        public const string TURNSCHUH = "ITG027";
        public const string SONNENBRILLE = "ITG028";
        public const string REISEPASS = "ITG029";
        public const string SAMMELKARTEN = "ITG052";
        public const string MUELL = "ITG060";

        // Test items (can be removed in production)
        public const string TEST_SPORTWAGEN = "ITG0011";
        public const string TEST_YELLOW_POINTS = "ITG0022";
        public const string TEST_GREEN_POINTS = "ITG0033";
        public const string TEST_ZEUGNIS = "ITG0044";
        public const string TEST_COUPON = "ITG0999";
        public const string TEST_SKIP_TURNS = "ITG9900";
        public const string TEST_DURABILITY_3 = "ITG9977";
        public const string TEST_DURABILITY_2 = "ITG9988";
        public const string TEST_DURABILITY_1 = "ITG9999";
    }

    // =========================================================================
    // EVENT CARDS (EVS_)
    // =========================================================================
    public static class Events
    {
        public const string PASSION = "EVS001";
        public const string DATING = "EVS012";
        public const string HANDSCHELLEN = "EVS016";
        public const string FESTIVAL = "EVS017";
        public const string HOCHZEIT = "EVS020";
        public const string FREMDGEHEN = "EVS022";
        public const string TRASHTALK = "EVS024";
        public const string BLACK_FRIDAY = "EVS030";
        public const string MEHR_PS = "EVS031";
        public const string WOCHENENDE = "EVS032";
        public const string RETAIL = "EVS033";
        public const string PAPIERKRAM = "EVS034";
        public const string PROBEFAHRT = "EVS035";
        public const string BUEROKRATIE = "EVS036";
        public const string PLAYTIME = "EVS038";
        public const string ATOMUNFALL = "EVS039";
        public const string THANKSGIVING = "EVS040";
        public const string VERSCHMUTZUNG = "EVS041";
        public const string ONE_NIGHT_STAND = "EVS042";
        public const string VALENTINSTAG = "EVS045";
        public const string DOUBLEDATE = "EVS046";
        public const string WINDOWSHOPPING = "EVS047";
        public const string AUSBORGEN = "EVS049";
        public const string WICHTELN = "EVS055";

        // System events
        public const string RESET_ALL_RELATIONSHIPS = "EVS800";
        public const string RESET_CURRENT_RELATIONSHIP = "EVS801";
        public const string GEFAENGNIS = "EVS990";
    }

    // =========================================================================
    // FIELD CARDS (FCP_)
    // =========================================================================
    public static class Fields
    {
        public const string MAIN_PASSION_POINTS = "FCP001";
        public const string CARD_INPUT = "FCP002";
        public const string STOP = "FCP003";
        public const string REST = "FCP004";
        public const string MINIGAME = "FCP005";
    }
}
