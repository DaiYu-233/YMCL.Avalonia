using System.Collections.Generic;

namespace YMCL.Public.Classes.Json;

public class AfdianSponsor
{
    public class Config
    {
    }

    public class Sponsor_plansItem
    {
        public double can_ali_agreement { get; set; }
        public string plan_id { get; set; }
        public double rank { get; set; }
        public string user_id { get; set; }
        public double status { get; set; }
        public string name { get; set; }
        public string pic { get; set; }
        public string desc { get; set; }
        public string price { get; set; }
        public double update_time { get; set; }
        public Timing timing { get; set; }
        public double pay_month { get; set; }
        public string show_price { get; set; }
        public string show_price_after_adjust { get; set; }
        public double has_coupon { get; set; }
        public List<string> coupon { get; set; }
        public double favorable_price { get; set; }
        public double independent { get; set; }
        public double permanent { get; set; }
        public double can_buy_hide { get; set; }
        public double need_address { get; set; }
        public double product_type { get; set; }
        public double sale_limit_count { get; set; }
        public string need_invite_code { get; set; }
        public double bundle_stock { get; set; }
        public double bundle_sku_select_count { get; set; }
        public Config config { get; set; }
        public double has_plan_config { get; set; }
        public List<string> shipping_fee_info { get; set; }
        public double expire_time { get; set; }
        public List<string> sku_processed { get; set; }
        public double rankType { get; set; }
    }

    public class Timing
    {
        public double timing_on { get; set; }
        public double timing_off { get; set; }
    }

    public class Current_plan
    {
        public double can_ali_agreement { get; set; }
        public string plan_id { get; set; }
        public double rank { get; set; }
        public string user_id { get; set; }
        public double status { get; set; }
        public string name { get; set; }
        public string pic { get; set; }
        public string desc { get; set; }
        public string price { get; set; }
        public double update_time { get; set; }
        public Timing timing { get; set; }
        public double pay_month { get; set; }
        public string show_price { get; set; }
        public string show_price_after_adjust { get; set; }
        public double has_coupon { get; set; }
        public List<string> coupon { get; set; }
        public double favorable_price { get; set; }
        public double independent { get; set; }
        public double permanent { get; set; }
        public double can_buy_hide { get; set; }
        public double need_address { get; set; }
        public double product_type { get; set; }
        public double sale_limit_count { get; set; }
        public string need_invite_code { get; set; }
        public double bundle_stock { get; set; }
        public double bundle_sku_select_count { get; set; }
        public Config config { get; set; }
        public double has_plan_config { get; set; }
        public List<string> shipping_fee_info { get; set; }
        public double expire_time { get; set; }
        public List<string> sku_processed { get; set; }
        public double rankType { get; set; }
    }

    public class User
    {
        public string user_id { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public string user_private_id { get; set; }
    }

    public class ListItem
    {
        public List<Sponsor_plansItem> sponsor_plans { get; set; }
        public Current_plan current_plan { get; set; }
        public string all_sum_amount { get; set; }
        public double first_pay_time { get; set; }
        public double last_pay_time { get; set; }
        public User user { get; set; }
    }

    public class Request
    {
        public string user_id { get; set; }
        public string @params { get; set; }
        public double ts { get; set; }
        public string sign { get; set; }
    }

    public class Data
    {
        public double total_count { get; set; }
        public double total_page { get; set; }
        public List<ListItem> list { get; set; }
        public Request request { get; set; }
    }

    public class Root
    {
        public double ec { get; set; }
        public string em { get; set; }
        public Data data { get; set; }
    }
}