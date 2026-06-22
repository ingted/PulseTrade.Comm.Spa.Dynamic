
const { chromium } = require("playwright");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  page.on("console", msg => console.log("BROWSER CONSOLE:", msg.type(), msg.text()));
  page.on("pageerror", err => console.log("BROWSER ERROR:", err.message));
  page.on("requestfailed", request => console.log("FAILED RESOURCE:", request.url(), request.failure().errorText));

  console.log("Navigating to chat page...");
  await page.goto("http://127.0.0.1:4431/chat", { waitUntil: "networkidle" });
  await page.waitForTimeout(1000);
  
  const durableEcho = page.locator("[data-participant-id=\"agent.durable-echo\"]");
  if (await durableEcho.count() > 0) {
    console.log("Found Durable Echo, clicking...");
    await durableEcho.click();
    await page.waitForTimeout(1000);
    
    // Type and send a message
    const draft = page.locator("[data-testid=\"chat-draft\"]");
    await draft.fill("init");
    const sendBtn = page.locator("[data-testid=\"chat-send\"]");
    await sendBtn.click();
    console.log("Sent message to Durable Echo.");
    
    await page.waitForTimeout(2000);
    
    // Click on the summary card to expand it (if it exists)
    const summaryCard = page.locator(".sdui-json-snippet");
    if (await summaryCard.count() > 0) {
      console.log("Found summary card, clicking to expand JSON...");
      await summaryCard.click();
      await page.waitForTimeout(1000);
    }
    
    // Check if Canvas Components are present
    const canvasContainers = page.locator("div[style*=\"border: 1px solid #5bc0de\"]");
    console.log("Found Canvas containers count:", await canvasContainers.count());

    await page.screenshot({ path: "chat_result_fixed.png" });
  } else {
    console.log("Could not find Durable Echo actor in chat sidebar.");
  }

  await browser.close();
})();

