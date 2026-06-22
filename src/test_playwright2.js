
const { chromium } = require("playwright");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  page.on("console", msg => console.log("BROWSER CONSOLE:", msg.type(), msg.text()));
  page.on("pageerror", err => console.log("BROWSER ERROR:", err.message));

  console.log("Navigating to chat page...");
  await page.goto("http://127.0.0.1:14484/chat", { waitUntil: "networkidle" });
  await page.waitForTimeout(1000);

  // The user says they added the actor to chat session.
  // We can try to add the participant using the UI, or send a message directly.
  // Wait, let us just check if there are any errors on load.
  
  // Try to find the Durable Echo actor in the sidebar and click it
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
    await page.screenshot({ path: "chat_result.png" });
  } else {
    console.log("Could not find Durable Echo actor in chat sidebar.");
    await page.screenshot({ path: "chat_result_no_actor.png" });
  }

  await browser.close();
})();

