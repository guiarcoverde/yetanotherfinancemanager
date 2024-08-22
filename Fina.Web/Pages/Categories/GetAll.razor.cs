using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Categories;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fina.Web.Pages.Categories;

public partial class GetAllCategoriesPage : ComponentBase
{
    #region Properties
    
    public bool IsBusy { get; set; } = false;
    public List<Category> Categories { get; set; } = [];
    
    
    #endregion

    #region Services
    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Inject] 
    public ICategoryHandler Handler { get; set; } = null!;

    [Inject] 
    public IDialogService DialogService { get; set; } = null!;

    #endregion

    #region Overrides
    

    protected override async Task OnInitializedAsync()
    {
        IsBusy = true;
        try
        {
            var request = new GetAllCategoriesRequest();
            var result = await Handler.GetAllAsync(request);
            if (!result.IsSuccess)
                Snackbar.Add(result.Message, Severity.Error);

            Categories = result.Data ?? [];

        }
        catch (Exception e)
        {
            Snackbar.Add(e.Message, Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
    

    #endregion

    public async void OnDeleteButtonClickedAsync(long id, string categoryTitle)
    {
        var result = await DialogService.ShowMessageBox("ATENÇÃO",
            $"Ao prosseguir, a categoria {categoryTitle} será removida. Deseja continuar?",
            yesText: "Excluir",
            cancelText: "Cancelar");

        if (result is true)
            await OnDeleteAsync(id, categoryTitle);
        
        StateHasChanged();
    }

    public async Task OnDeleteAsync(long id, string title)
    {
        try
        {
            var request = new DeleteCategoryRequest
            {
                Id = id
            };

            await Handler.DeleteAsync(request);
            Categories.RemoveAll(x => x.Id == id);
            Snackbar.Add($"Categoria {title} removida.", Severity.Info);
        }
        catch (Exception e)
        {
            Snackbar.Add(e.Message, Severity.Error);
        }
    }

}