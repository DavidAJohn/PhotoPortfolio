using Humanizer;
using MudBlazor;
using PhotoPortfolio.Client.Components;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Client.Pages.Admin;

public partial class Orders
{
    private List<OrderDetailsDto> orders { get; set; } = null!;
    private string errorMessage = "";

    private OrderSpecificationParams orderParams = new();
    private List<DropdownItem> sortOptions = new();
    private string sortTitle = "Sorted By";

    private List<DropdownItem> filterTimeOptions = new();
    private string filterTimeTitle = "In Last Year";

    private List<DropdownItem> filterStatusOptions = new();
    private string filterStatusTitle = "Any Status";

    private string expandedOrderId = "";
    private string stripeDashboardUrl => config["Stripe_DashboardRootUrl"] ?? "https://dashboard.stripe.com/test/payments/";

    private Dictionary<string, string> prefsMetadata = new();
    private bool excludeIncompleteChecked = false;
    private string savePrefsSpinnerVisible = "invisible";

    protected override async Task OnInitializedAsync()
    {
        orderParams.SortBy = "ordercreated";
        orderParams.SortOrder = "desc";
        sortTitle = "Order Date (Most Recent First)";

        orderParams.InLastNumberOfDays = 365;
        filterTimeTitle = "In Last Year";

        orderParams.Status = "";
        filterStatusTitle = "Any Status";

        await GetSitePreferences();
        await GetOrders();
    }

    protected override void OnInitialized()
    {
        SetSortOptions();
        SetFilterTimeOptions();
        SetFilterStatusOptions();
    }

    private void SetSortOptions()
    {
        sortOptions.Clear();

        sortOptions.Add(new DropdownItem
        {
            OptionName = "Order Date (Most Recent First)",
            OptionRef = "ordercreated_desc"
        });

        sortOptions.Add(new DropdownItem
        {
            OptionName = "Order Date (Oldest First)",
            OptionRef = "ordercreated_asc"
        });
    }

    private void SetFilterTimeOptions()
    {
        filterTimeOptions.Clear();

        filterTimeOptions.Add(new DropdownItem
        {
            OptionName = "In Last 7 Days",
            OptionRef = "7"
        });

        filterTimeOptions.Add(new DropdownItem
        {
            OptionName = "In Last 14 Days",
            OptionRef = "14"
        });

        filterTimeOptions.Add(new DropdownItem
        {
            OptionName = "In Last 30 Days",
            OptionRef = "30"
        });

        filterTimeOptions.Add(new DropdownItem
        {
            OptionName = "In Last 6 Months",
            OptionRef = "180"
        });

        filterTimeOptions.Add(new DropdownItem
        {
            OptionName = "In Last Year",
            OptionRef = "365"
        });

        filterTimeOptions.Add(new DropdownItem
        {
            OptionName = "All Orders",
            OptionRef = "36500" // 100 years
        });
    }

    private void SetFilterStatusOptions(bool excludePaymentIncomplete = false)
    {
        filterStatusOptions.Clear();

        filterStatusOptions.Add(new DropdownItem
        {
            OptionName = "Any Status",
            OptionRef = "Any"
        });

        string[] statusNames = Enum.GetNames<OrderStatus>();

        foreach (string name in statusNames)
        {
            if (!excludePaymentIncomplete || (excludePaymentIncomplete && name != "PaymentIncomplete"))
            {
                filterStatusOptions.Add(new DropdownItem
                {
                    OptionName = name.Humanize(LetterCasing.Title), // eg. "Awaiting Approval"
                    OptionRef = name
                });
            }
        }
    }

    private async Task GetOrders()
    {
        try
        {
            orders = await adminService.GetOrdersAsync(orderParams);

            if (orders is null)
            {
                errorMessage = "Could not retrieve list of orders";
            }
            else
            {
                errorMessage = "";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            errorMessage = "Could not retrieve list of orders";
        }
    }

    private async Task SelectedSortOption(DropdownItem selectedOption)
    {
        string sortBy = selectedOption.OptionRef;
        sortTitle = selectedOption.OptionName;

        orderParams.SortBy = "ordercreated";

        orderParams.SortOrder = sortBy switch
        {
            "ordercreated_asc" => "asc",
            "ordercreated_desc" => "desc",
            _ => "desc"
        };

        orders = await adminService.GetOrdersAsync(orderParams);
    }

    private async Task SelectedTimeFilterOption(DropdownItem selectedOption)
    {
        try
        {
            filterTimeTitle = selectedOption.OptionName;

            orderParams.InLastNumberOfDays = int.Parse(selectedOption.OptionRef);

            orders = await adminService.GetOrdersAsync(orderParams);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            errorMessage = "Could not retrieve list of orders";
        }
    }

    private async Task SelectedStatusFilterOption(DropdownItem selectedOption)
    {
        try
        {
            filterStatusTitle = selectedOption.OptionName;

            if (selectedOption.OptionRef != "Any")
            {
                orderParams.Status = selectedOption.OptionRef;
            }
            else
            {
                orderParams.Status = "";
            }

            orders = await adminService.GetOrdersAsync(orderParams);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            errorMessage = "Could not retrieve list of orders";
        }
    }

    private void ViewOrderDetails(string orderId)
    {
        if (orderId == expandedOrderId)
        {
            expandedOrderId = "";
        }
        else
        {
            expandedOrderId = orderId;
        }
    }

    private async Task CheckBoxExcludeIncomplete_CheckedChanged(bool excludeIncomplete)
    {
        if (excludeIncomplete == true)
        {
            if (filterStatusTitle == "Payment Incomplete")
            {
                orderParams.Status = "";
                filterStatusTitle = "Any Status";
            }
        }

        SetFilterStatusOptions(excludeIncomplete);
        excludeIncompleteChecked = excludeIncomplete;
        orderParams.ExcludePaymentIncomplete = excludeIncomplete;
        await UpdateSitePreferences(excludeIncomplete);
        await GetOrders();
    }

    private async Task GetSitePreferences()
    {
        try
        {
            var preferences = await adminService.GetSitePreferencesAsync();
            prefsMetadata = preferences.Metadata;

            if (prefsMetadata.ContainsKey("ExcludeIncompleteFromOrderSearch"))
            {
                bool boolOut;

                if (bool.TryParse(prefsMetadata["ExcludeIncompleteFromOrderSearch"], out boolOut))
                {
                    excludeIncompleteChecked = Convert.ToBoolean(prefsMetadata["ExcludeIncompleteFromOrderSearch"]);
                    orderParams.ExcludePaymentIncomplete = excludeIncompleteChecked;
                }
            }

            errorMessage = "";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            errorMessage = "Could not retrieve site preferences";
        }
    }

    private async Task UpdateSitePreferences(bool excludeIncomplete)
    {
        savePrefsSpinnerVisible = "visible";

        try
        {
            var preferences = await adminService.GetSitePreferencesAsync();
            prefsMetadata = preferences.Metadata;

            if (prefsMetadata.ContainsKey("ExcludeIncompleteFromOrderSearch"))
            {
                prefsMetadata["ExcludeIncompleteFromOrderSearch"] = excludeIncomplete.ToString();
            }
            else
            {
                prefsMetadata.Add("ExcludeIncompleteFromOrderSearch", excludeIncomplete.ToString());
            }

            var response = await adminService.UpdateSitePreferencesAsync(preferences);

            if (response)
            {
                errorMessage = "";
            }
            else
            {
                Console.WriteLine("Could not update site preferences");
                errorMessage = "Could not update site preferences";
            }

            savePrefsSpinnerVisible = "invisible";
        }
        catch (Exception ex)
        {
            savePrefsSpinnerVisible = "invisible";
            Console.WriteLine(ex.Message);
            errorMessage = "Could not update site preferences";
        }
    }

    private async Task OnApproveClick(string orderId)
    {
        var parameters = new DialogParameters();
        parameters.Add("ContentText", "Are you sure? The order will be sent to the Prodigi Print API immediately");
        parameters.Add("ButtonText", "Yes, approve it");
        parameters.Add("Color", Color.Success);

        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small };

        var dialog = DialogService.Show<ConfirmDialog>("Approve Order", parameters, options);
        var result = await dialog.Result;

        if (!result.Cancelled) // user must have clicked the approve button - cancelling just closes the confirm dialog
        {
            var orderApproved = await adminService.ApproveOrderAsync(orderId);

            if (orderApproved)
            {
                await InvokeAsync(StateHasChanged);
                Snackbar.Add($"This order has now been approved", Severity.Success);
            }
            else
            {
                Snackbar.Add("The order could NOT be approved", Severity.Error);
            }
        }
    }
}
