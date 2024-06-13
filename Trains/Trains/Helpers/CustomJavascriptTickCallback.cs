using ChartJs.Blazor.Common.Handlers;
using ChartJs.Blazor.Interop;
using Microsoft.JSInterop;

namespace Trains.Helpers
{
    public class CustomJavascriptTickCallback : IMethodHandler<AxisTickCallback>
    {
        private readonly IJSRuntime _jsRuntime;

        public CustomJavascriptTickCallback(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public string MethodName => "callback";

        public async Task<string> Handle(double value)
        {
            return await _jsRuntime.InvokeAsync<string>("formatDate", value);
        }
    }
}
