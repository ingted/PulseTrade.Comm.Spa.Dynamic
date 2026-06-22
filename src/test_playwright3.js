
const { chromium } = require("playwright");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  page.on("requestfailed", request => {
    console.log("FAILED RESOURCE:", request.url(), request.failure().errorText);
  });
  
  page.on("response", response => {
    if (response.status() === 404) {
      console.log("404 RESOURCE:", response.url());
    }
  });

  console.log("Navigating to chat page...");
  await page.goto("http://127.0.0.1:14484/chat", { waitUntil: "networkidle" });
  await page.waitForTimeout(1000);

  await browser.close();
})();

