
const { chromium } = require("playwright");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  console.log("Navigating to chat page...");
  await page.goto("http://127.0.0.1:14484/chat", { waitUntil: "networkidle" });
  await page.waitForTimeout(2000);

  // Take initial screenshot
  await page.screenshot({ path: "initial.png" });

  // Output all text to see what is on the page
  console.log("Page title:", await page.title());
  
  // Dump the HTML
  const html = await page.content();
  require("fs").writeFileSync("page_dump.html", html);

  // We need to figure out how to add an actor and send a message.
  // We can evaluate JS to simulate adding the actor if we know the DOM structure.
  console.log("Done. Check page_dump.html and initial.png");
  
  await browser.close();
})();

