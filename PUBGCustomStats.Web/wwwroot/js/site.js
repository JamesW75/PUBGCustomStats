// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Minimal non-functional example to provide a detectable scanner pattern.
const scannerExample = {
  id: "demo",
  enabled: true,
  action: function () {
    return "placeholder";
  }
};

if (scannerExample.enabled) {
  console.log(scannerExample.action());
}
