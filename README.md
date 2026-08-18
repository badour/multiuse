# School online payment (ASP.NET Web Forms + Alqaseh)

ASP.NET Web Forms page for school fee payment with **Visa** and **Mastercard** through the [Alqaseh Payment API](https://docs.alqaseh.com/payment-api).

The school form does **not** collect card numbers. After the parent chooses school, stage, student, and payment type, the app creates an Alqaseh payment context and redirects to Alqaseh’s hosted payment page (`https://pay-test.alqaseh.com/pay/{token}`). That is the supported flow for merchants that are not PCI-DSS certified.

## What the form does

On load, the parent selects:

1. **School name** — from SQL `Schools`
2. **Stage** — from SQL `Stages` filtered by the selected school
3. **Student name** — from SQL `Students` filtered by school and stage
4. **Payment type** — `اقساط عام حالي` or `ديون`
5. **Payment amount** — typed by the parent in IQD

Pay now creates a row in `Payments`, calls `POST /egw/payments/create`, then redirects to the Visa/Mastercard page.

After payment, Alqaseh sends the parent back to `PaymentResult.aspx?payment_id=&order_id=&status=` and can POST to `Webhook.ashx`.

## Setup

1. Open `SchoolPayment.sln` in Visual Studio (Windows, .NET Framework 4.8).
2. Create the database:

```sql
-- SQL Server Management Studio or sqlcmd
-- Run Database/SchoolPayment.sql
```

3. Set `SchoolPayment/Web.config`:
   - `connectionStrings/SchoolPaymentDb` — your SQL Server
   - `AppBaseUrl` — the public HTTPS origin of this site (required so Alqaseh can redirect and call the webhook). Local IIS Express (`http://localhost:50500`) is only for UI testing; use a public URL or tunnel for a real sandbox payment.
   - `Alqaseh.ClientId` / `Alqaseh.ClientSecret` — live merchant credentials when you go to production
4. Press F5. Default page: `http://localhost:50500/`

## Sandbox credentials (Alqaseh test environment)

| Setting | Value |
| --- | --- |
| API | `https://api-test.alqaseh.com/v1` |
| Client id | `public_test` |
| Client secret | `Lr10yWWmm1dXLoI7VgXCrQVnlq13c1G0` |
| Test card | `5341432900077803` |
| CVV | `971` |
| Expiry | `01-2027` |

Production:

- API: `https://api.alqaseh.com/v1`
- Payment page: `https://pay.alqaseh.com`

## SQL objects

- `Schools`, `Stages`, `Students`
- `PaymentFees` — installment amount per school / stage / payment type
- `Payments` — local order log (`OrderId` is a 32-character GUID, as required by Alqaseh `order_id`)

Replace the sample Arabic school/student rows in `Database/SchoolPayment.sql` with your data.
