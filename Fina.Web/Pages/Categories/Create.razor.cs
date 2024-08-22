using Fina.Core.Handlers;
using Fina.Core.Requests.Categories;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fina.Web.Pages.Categories;

public partial class CreateCategoryPage : ComponentBase
{
    #region Properties

    public bool IsBusy { get; set; } = false;
    public CreateCategoryRequest InputModel { get; set; } = new();

    #endregion

    #region Services

    [Inject] public ICategoryHandler Handler { get; set; } = null!;

    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    #endregion

    #region Methods

    public async Task OnValidSubmitAsync()
    {
        IsBusy = true;

        try
        {
            var result = await Handler.CreateAsync(InputModel);
            if (!result.IsSuccess)
            {
                Snackbar.Add(result.Message, Severity.Error);
            }
            
            Snackbar.Add(result.Message, Severity.Success);
            NavigationManager.NavigateTo("/categorias");
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    #endregion
}