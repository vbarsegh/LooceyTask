﻿//This service will handle all requests to Xero using the stored token.
using Application.DTOs;
using Application_Layer.DTO.Customers;
using Application_Layer.Interfaces;
using Application_Layer.Interfaces_Repository;
using Domain_Layer.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Application_Layer.Services
{
    public class XeroApiManager : IXeroApiManager
    {
        private readonly IXeroTokenRepository _tokenRepository;
        private readonly IXeroAuthService _xeroAuthService; // ✅ add this
        private readonly IConfiguration _config;
        public XeroApiManager(IXeroTokenRepository tokenRepository, IXeroAuthService xeroAuthService, IConfiguration config)
        {
            _tokenRepository = tokenRepository;
            _xeroAuthService = xeroAuthService;
            _config = config;
        }
     

        /// <summary>
        /// Karevor:ete petq exav piti regresh token anenq
        private async Task<string> GetValidAccessTokenAsync()
        {
            var token = await _tokenRepository.GetTokenAsync();

            if (token == null)
                throw new Exception("No Xero token found. Please log in again.");

            // Check if token is expired or close to expiring
            if (token.UpdatedAt.AddSeconds(token.ExpiresIn) < DateTime.UtcNow)
            {
                // Refresh the token
                var newToken = await _xeroAuthService.RefreshAccessTokenAsync(token.RefreshToken);
                await _tokenRepository.SaveTokenAsync(newToken);
                return newToken.AccessToken;
            }

            return token.AccessToken;
        }
        // Helper: Get access token from DB
        private async Task<string> GetAccessTokenAsync()
        {
            var token = await _tokenRepository.GetTokenAsync();
            if (token == null) throw new Exception("No Xero token found.");
            // TODO: refresh token if expired
            return token.AccessToken;
        }

        #region Customers
        /// ////////////////////
        public async Task<string> GetConnectionsAsync()
        {
            var accessToken = await GetValidAccessTokenAsync();

            var client = new RestClient("https://api.xero.com/connections");
            var request = new RestRequest();
            request.Method = Method.Get;
            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("Accept", "application/json");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Failed to get connections: {response.Content}");

            return response.Content; // JSON that includes your tenantId
        }
        ///////////////////////////////
        public async Task<string> GetCustomersAsync()
        {
            //var accessToken = await GetAccessTokenAsync();
            var accessToken = await GetValidAccessTokenAsync();
            if (accessToken == null) throw new Exception("No Xero token found");
            // 2. Create a request to Xero API
            var client = new RestClient("https://api.xero.com/api.xro/2.0/Contacts");//Creates a client to call Xero API’s Contacts endpoint (customers).
            var request = new RestRequest();
            request.Method = Method.Get;
            request.AddHeader("Authorization", $"Bearer {accessToken}");//Adds the access token in the HTTP header so Xero knows who you are.
            request.AddHeader("xero-tenant-id", _config["XeroSettings:TenantId"]);
            request.AddHeader("Accept", "application/json");
            // 3. Send request and get response// 3. Send request and get response
            var response = await client.ExecuteAsync(request);//Sends the GET request to Xero and waits for the response asynchronously.
            // 4. Check if the request was successful
            if (!response.IsSuccessful)
                throw new Exception($"Xero API error: {response.Content}");

            return response.Content;
        }

        public async Task<string> CreateCustomerAsync(CustomerCreateDto customer)
        {
            //these only talk to Xero API, not your local database yet.
            //var accessToken = await GetAccessTokenAsync();
            var accessToken = await GetValidAccessTokenAsync();
            var client = new RestClient("https://api.xero.com/api.xro/2.0/Contacts");
            var request = new RestRequest();
            request.Method = Method.Post;

            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("xero-tenant-id", _config["XeroSettings:TenantId"]);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                Name = customer.Name,
                EmailAddress = customer.Email,
                Phones = new[]
                {
                    new { PhoneType = "DEFAULT", PhoneNumber = customer.Phone }
                },
                        Addresses = new[]
                {
                    new { AddressType = "STREET", AddressLine1 = customer.Address }
                }
            };
            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Xero API error: {response.Content}");

            return response.Content;
        }
        //The difference is the URL and whether XeroId exists. That’s how Xero knows “create” vs “update.”
        public async Task<string> UpdateCustomerAsync(CustomerCreateDto customer)
        {
            //var accessToken = await GetAccessTokenAsync();
            var accessToken = await GetValidAccessTokenAsync();
            var client = new RestClient($"https://api.xero.com/api.xro/2.0/Contacts/{customer.XeroId}");
            var request = new RestRequest();
            request.Method = Method.Put;

            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("xero-tenant-id", _config["XeroSettings:TenantId"]);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                Name = customer.Name,
                EmailAddress = customer.Email,
                Phones = new[]
                 {
                    new { PhoneType = "DEFAULT", PhoneNumber = customer.Phone }
                },
                        Addresses = new[]
                 {
                    new { AddressType = "STREET", AddressLine1 = customer.Address }
                }
            };
            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Xero API error: {response.Content}");

            return response.Content;
        }

        #endregion

        #region Invoices

        public async Task<string> GetInvoicesAsync()
        {
            //var accessToken = await GetAccessTokenAsync();
            var accessToken = await GetValidAccessTokenAsync();
            var client = new RestClient("https://api.xero.com/api.xro/2.0/Invoices");
            var request = new RestRequest();
            request.Method = Method.Get;

            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("xero-tenant-id", _config["XeroSettings:TenantId"]);

            request.AddHeader("Accept", "application/json");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Xero API error: {response.Content}");

            return response.Content;
        }

        #region Invoices

        public async Task<string> CreateInvoiceAsync(InvoiceCreateDto invoice)
        {
            //var accessToken = await GetAccessTokenAsync();
            var accessToken = await GetValidAccessTokenAsync();
            var client = new RestClient("https://api.xero.com/api.xro/2.0/Invoices");
            var request = new RestRequest();
            request.Method = Method.Post;

            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("xero-tenant-id", _config["XeroSettings:TenantId"]);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");

            // Xero requires an object named "Invoices" containing a list
            var body = new
            {
                Invoices = new[]
                {
            new
            {
                Type = "ACCREC", // Accounts Receivable (customer invoice)
                Contact = new { ContactID = invoice.ContactId },
                LineItems = new[]
                {
                    new { Description = invoice.Description, Quantity = 1, UnitAmount = invoice.Amount }
                },
                DueDate = invoice.DueDate.ToString("yyyy-MM-dd"),
                Status = "AUTHORISED"
            }
        }
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Xero API error: {response.Content}");

            return response.Content;
        }

        public async Task<string> UpdateInvoiceAsync(InvoiceCreateDto invoice)
        {
            if (string.IsNullOrEmpty(invoice.XeroId))
                throw new ArgumentException("XeroId is required to update an invoice.");

            //var accessToken = await GetAccessTokenAsync();
            var accessToken = await GetValidAccessTokenAsync();
            var client = new RestClient($"https://api.xero.com/api.xro/2.0/Invoices/{invoice.XeroId}");
            var request = new RestRequest();
            request.Method = Method.Put; // Xero also accepts PUT for updates

            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("xero-tenant-id", _config["XeroSettings:TenantId"]);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                Invoices = new[]
                {
            new
            {
                InvoiceID = invoice.XeroId,
                LineItems = new[]
                {
                    new { Description = invoice.Description, Quantity = 1, UnitAmount = invoice.Amount }
                },
                DueDate = invoice.DueDate.ToString("yyyy-MM-dd"),
                Status = "AUTHORISED"
            }
        }
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Xero API error: {response.Content}");

            return response.Content;
        }

        #endregion


        #endregion
        #region Quotes

        // GET all quotes from Xero
        public async Task<string> GetQuotesAsync()
        {
            //var accessToken = await GetAccessTokenAsync();
            var accessToken = await GetValidAccessTokenAsync();
            var client = new RestClient("https://api.xero.com/api.xro/2.0/Quotes");
            var request = new RestRequest();
            request.Method = Method.Get;
            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("xero-tenant-id", _config["XeroSettings:TenantId"]);

            request.AddHeader("Accept", "application/json");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Xero API error: {response.Content}");

            return response.Content;
        }

        // CREATE a quote in Xero
        public async Task<string> CreateQuoteAsync(QuoteCreateDto quote)
        {
            //var accessToken = await GetAccessTokenAsync();
            var accessToken = await GetValidAccessTokenAsync();
            var client = new RestClient("https://api.xero.com/api.xro/2.0/Quotes");
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("xero-tenant-id", _config["XeroSettings:TenantId"]);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                CustomerName = quote.CustomerName,
                Date = quote.Date,
                LineItems = quote.LineItems
            };

            request.AddJsonBody(body);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Xero API error: {response.Content}");

            return response.Content;
        }

        // UPDATE a quote in Xero
        public async Task<string> UpdateQuoteAsync(QuoteCreateDto quote)
        {
            if (string.IsNullOrEmpty(quote.XeroId))
                throw new ArgumentException("XeroId is required to update a quote.");

            //var accessToken = await GetAccessTokenAsync();
            var accessToken = await GetValidAccessTokenAsync();
            var client = new RestClient($"https://api.xero.com/api.xro/2.0/Quotes/{quote.XeroId}");
            var request = new RestRequest();
            request.Method = Method.Put; // Xero requires PUT for updates
            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("xero-tenant-id", _config["XeroSettings:TenantId"]);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                CustomerName = quote.CustomerName,
                Date = quote.Date,
                LineItems = quote.LineItems
            };

            request.AddJsonBody(body);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Xero API error: {response.Content}");

            return response.Content;
        }

        #endregion


    }
}
