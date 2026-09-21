using System.Net;
using Carter;

namespace Payment_Service.Features.Payments.ReturnPages;

/// <summary>
/// Where Stripe sends the customer after the hosted payment page. These pages are only something
/// to look at: they never mark an order paid and never read the query string into the page. The
/// app is expected to close its webview as soon as it sees one of these URLs and ask the status
/// endpoint what actually happened.
/// </summary>
public sealed class PaymentReturnPagesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(PaymentRoutes.ReturnSuccess, (HttpResponse response) => Page(response, ReturnPage.Success))
            .AllowAnonymous()
            .WithName("PaymentReturnSuccess")
            .ExcludeFromDescription();

        app.MapGet(PaymentRoutes.ReturnCancel, (HttpResponse response) => Page(response, ReturnPage.Cancelled))
            .AllowAnonymous()
            .WithName("PaymentReturnCancel")
            .ExcludeFromDescription();
    }

    private static IResult Page(HttpResponse response, ReturnPage page)
    {
        // The URL carries the Stripe session id, so it must not leak to other sites or be cached.
        response.Headers.CacheControl = "no-store";
        response.Headers["Referrer-Policy"] = "no-referrer";
        response.Headers["X-Content-Type-Options"] = "nosniff";
        response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; img-src data:; base-uri 'none'; form-action 'none'; frame-ancestors 'none'";

        return Results.Content(ReturnPageHtml.Render(page), "text/html; charset=utf-8", statusCode: (int)HttpStatusCode.OK);
    }
}

public enum ReturnPage
{
    Success,
    Cancelled
}

internal static class ReturnPageHtml
{
    public static string Render(ReturnPage page)
    {
        var success = page is ReturnPage.Success;

        // Deliberately careful wording: reaching this page means Stripe accepted the payment, but
        // the order is only confirmed once the backend has heard from Stripe.
        var (arabicTitle, arabicBody, englishTitle, englishBody) = success
            ? ("تم استلام عملية الدفع",
               "جارٍ تأكيد طلبك الآن. يمكنك العودة إلى التطبيق لمتابعة حالة الطلب.",
               "Payment received",
               "We are confirming your order now. Return to the app to follow it.")
            : ("لم تكتمل عملية الدفع",
               "لم يتم خصم أي مبلغ. يمكنك العودة إلى التطبيق والمحاولة مرة أخرى.",
               "Payment not completed",
               "You were not charged. Return to the app to try again.");

        var icon = success
            ? """<svg viewBox="0 0 48 48" aria-hidden="true"><circle cx="24" cy="24" r="22" fill="none" stroke="currentColor" stroke-width="3"/><path d="M14 25l7 7 13-15" fill="none" stroke="currentColor" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round"/></svg>"""
            : """<svg viewBox="0 0 48 48" aria-hidden="true"><circle cx="24" cy="24" r="22" fill="none" stroke="currentColor" stroke-width="3"/><path d="M17 17l14 14M31 17L17 31" fill="none" stroke="currentColor" stroke-width="3.5" stroke-linecap="round"/></svg>""";

        var tone = success ? "success" : "cancelled";

        return $$"""
            <!doctype html>
            <html lang="ar" dir="rtl">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <meta name="robots" content="noindex">
            <title>{{englishTitle}} · Flower</title>
            <style>
              :root {
                --bg: #f7f5f2; --card: #ffffff; --ink: #1f1d1a; --muted: #6b6560; --line: #e7e2dc;
                --success: #2f7d4f; --cancelled: #a4452c;
                color-scheme: light dark;
              }
              @media (prefers-color-scheme: dark) {
                :root { --bg: #151413; --card: #1e1c1a; --ink: #f2eee9; --muted: #a39c95; --line: #34302c;
                        --success: #6cc68f; --cancelled: #eb8a6d; }
              }
              * { box-sizing: border-box; }
              body {
                margin: 0; min-height: 100vh; display: grid; place-items: center;
                padding: 24px 16px; background: var(--bg); color: var(--ink);
                font-family: system-ui, -apple-system, "Segoe UI", Tahoma, Arial, sans-serif;
              }
              main {
                width: 100%; max-width: 420px; background: var(--card); border: 1px solid var(--line);
                border-radius: 20px; padding: 36px 28px 28px; text-align: center;
              }
              .icon { width: 64px; height: 64px; margin: 0 auto 20px; color: var(--{{tone}}); }
              .icon svg { width: 100%; height: 100%; }
              h1 { font-size: 1.35rem; margin: 0 0 10px; line-height: 1.4; }
              p { margin: 0; color: var(--muted); line-height: 1.7; font-size: 1rem; }
              .en { margin-top: 22px; padding-top: 20px; border-top: 1px solid var(--line); }
              .en h2 { font-size: 1.05rem; margin: 0 0 6px; color: var(--ink); }
              .en p { font-size: 0.93rem; }
              .brand { margin-top: 26px; font-size: 0.8rem; color: var(--muted); letter-spacing: 0.08em; }
            </style>
            </head>
            <body>
            <main>
              <div class="icon">{{icon}}</div>
              <h1>{{arabicTitle}}</h1>
              <p>{{arabicBody}}</p>
              <div class="en" dir="ltr" lang="en">
                <h2>{{englishTitle}}</h2>
                <p>{{englishBody}}</p>
              </div>
              <div class="brand" dir="ltr">FLOWER</div>
            </main>
            </body>
            </html>
            """;
    }
}
