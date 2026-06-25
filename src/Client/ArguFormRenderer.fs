namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
module ArguFormRenderer =

    [<JavaScriptExport>]
    let Register () =
        JS.Inline("""
(function () {
  if (!window.PulseTradeRegisterAppendInputRenderer || !window.PulseTradeRegisterAddKeyRenderer) {
    console.warn("PulseTrade Argu form renderer skipped: PTCS registry is unavailable.");
    return;
  }

  var sampleTypeName = "PulseTrade.Comm.Spa.Dynamic.SampleArgu";
  var schemas = window.PulseTradeDynamicArguSchemas || {};
  schemas[sampleTypeName] = {
    schema: "fskynet-sdui",
    formMode: "argu-form",
    duTypeName: sampleTypeName,
    unionCases: [
      { name: "Say", label: "Say", arguName: "--say", fields: [
        { name: "text", label: "Text", kind: "text", arguName: "--say", options: [], items: [] }
      ] },
      { name: "SetCount", label: "Set Count", arguName: "--set-count", fields: [
        { name: "count", label: "Count", kind: "number", arguName: "--set-count", options: [], items: [] }
      ] },
      { name: "Mode", label: "Mode", arguName: "--mode", fields: [
        { name: "mode", label: "Mode", kind: "enum", arguName: "--mode", options: ["Fast", "Safe", "Audit"], items: [] }
      ] },
      { name: "At", label: "Tuple At", arguName: "--at", fields: [
        { name: "at", label: "At", kind: "tuple", arguName: "--at", options: [], items: [
          { name: "symbol", label: "1. symbol", kind: "text", arguName: "", options: [], items: [] },
          { name: "quantity", label: "2. quantity", kind: "number", arguName: "", options: [], items: [] }
        ] }
      ] },
      { name: "Tag", label: "Tag List", arguName: "--tag", fields: [
        { name: "tag", label: "Tags", kind: "list", arguName: "--tag", options: [], items: [
          { name: "tagItem", label: "Tag", kind: "text", arguName: "", options: [], items: [] }
        ] }
      ] },
      { name: "Verbose", label: "Verbose", arguName: "--verbose", fields: [
        { name: "verbose", label: "Verbose", kind: "bool", arguName: "--verbose", options: [], items: [] }
      ] }
    ]
  };
  window.PulseTradeDynamicArguSchemas = schemas;

  function el(tag, className, testId) {
    var node = document.createElement(tag);
    if (className) node.className = className;
    if (testId) node.setAttribute("data-testid", testId);
    return node;
  }

  function labelText(text) {
    var node = el("label", "dynamic-argu-label", null);
    node.textContent = text || "";
    return node;
  }

  function quoteArg(value) {
    var text = value == null ? "" : String(value);
    if (text.length === 0) return "\"\"";
    if (/\s|"/.test(text)) return "\"" + text.replace(/\\/g, "\\\\").replace(/"/g, "\\\"") + "\"";
    return text;
  }

  function appendField(parts, field, values) {
    values = (values || []).map(function (value) { return value == null ? "" : String(value).trim(); })
      .filter(function (value) { return value.length > 0; });

    if (field.kind === "bool") {
      var value = values.length > 0 ? values[0].toLowerCase() : "";
      if (value === "true" || value === "1" || value === "yes") parts.push(field.arguName);
      return;
    }

    if (field.kind === "list") {
      values.forEach(function (value) {
        parts.push(field.arguName);
        parts.push(quoteArg(value));
      });
      return;
    }

    if (field.kind === "tuple") {
      if (values.length > 0) {
        parts.push(field.arguName);
        values.forEach(function (value) { parts.push(quoteArg(value)); });
      }
      return;
    }

    if (values.length > 0) {
      parts.push(field.arguName);
      parts.push(quoteArg(values[0]));
    }
  }

  function selectedValues(field, row) {
    if (field.kind === "tuple") {
      return Array.from(row.querySelectorAll("[data-dynamic-argu-tuple-item]")).map(function (input) { return input.value; });
    }

    if (field.kind === "list") {
      return Array.from(row.querySelectorAll("[data-dynamic-argu-list-item]")).map(function (input) { return input.value; });
    }

    if (field.kind === "bool") {
      var check = row.querySelector("[data-dynamic-argu-input]");
      return [check && check.checked ? "true" : "false"];
    }

    var input = row.querySelector("[data-dynamic-argu-input]");
    return [input ? input.value : ""];
  }

  function buildRawArgu(unionCase, form) {
    var parts = [];
    unionCase.fields.forEach(function (field) {
      var row = form.querySelector('[data-dynamic-argu-field="' + field.name + '"]');
      appendField(parts, field, row ? selectedValues(field, row) : []);
    });
    return parts.join(" ");
  }

  function renderScalar(field, inputType, testId) {
    var input = el("input", "dynamic-argu-input", testId);
    input.type = inputType;
    input.setAttribute("data-dynamic-argu-input", "true");
    return input;
  }

  function renderField(field) {
    var row = el("div", "dynamic-argu-field", "dynamic-argu-field-" + field.name);
    row.setAttribute("data-dynamic-argu-field", field.name);
    row.setAttribute("data-dynamic-argu-kind", field.kind);
    row.appendChild(labelText(field.label || field.name));

    if (field.kind === "number") {
      row.appendChild(renderScalar(field, "number", "dynamic-argu-number-" + field.name));
    } else if (field.kind === "enum") {
      var select = el("select", "dynamic-argu-select", "dynamic-argu-enum-" + field.name);
      select.setAttribute("data-dynamic-argu-input", "true");
      (field.options || []).forEach(function (optionValue) {
        var option = document.createElement("option");
        option.value = optionValue;
        option.textContent = optionValue;
        select.appendChild(option);
      });
      row.appendChild(select);
    } else if (field.kind === "tuple") {
      var tuple = el("div", "dynamic-argu-tuple", "dynamic-argu-tuple-" + field.name);
      (field.items || []).forEach(function (item, index) {
        var itemRow = el("div", "dynamic-argu-tuple-item", "dynamic-argu-tuple-item-" + field.name + "-" + (index + 1));
        itemRow.appendChild(labelText((index + 1) + ". " + (item.label || item.name)));
        var input = renderScalar(item, item.kind === "number" ? "number" : "text", null);
        input.setAttribute("data-dynamic-argu-tuple-item", String(index + 1));
        itemRow.appendChild(input);
        tuple.appendChild(itemRow);
      });
      row.appendChild(tuple);
    } else if (field.kind === "list") {
      var list = el("div", "dynamic-argu-list", "dynamic-argu-list-" + field.name);
      var add = el("button", "dynamic-argu-add-list-item", "dynamic-argu-list-add-" + field.name);
      add.type = "button";
      add.textContent = "Add";
      var addInput = function () {
        var input = renderScalar(field, "text", "dynamic-argu-list-item-" + field.name);
        input.setAttribute("data-dynamic-argu-list-item", "true");
        list.insertBefore(input, add);
      };
      add.addEventListener("click", addInput);
      list.appendChild(add);
      row.appendChild(list);
      addInput();
    } else if (field.kind === "bool") {
      row.appendChild(renderScalar(field, "checkbox", "dynamic-argu-bool-" + field.name));
    } else {
      row.appendChild(renderScalar(field, "text", "dynamic-argu-text-" + field.name));
    }

    return row;
  }

  function unionCaseNamesFromContext(ctx, schema) {
    var allowed = Array.isArray(ctx.unionCaseNames) ? ctx.unionCaseNames : [];
    if (allowed.length === 0) return schema.unionCases.map(function (item) { return item.name; });
    return allowed.map(String);
  }

  window.PulseTradeRegisterAddKeyRenderer("dynamic-argu-add-key", 100, function (ctx) {
    var shape = String(ctx.shape || "").toLowerCase();
    if (shape !== "actor-dynamic" && shape !== "actor-argu") return null;

    var root = el("div", "dynamic-argu-add-key", "dynamic-argu-add-key");
    var actor = el("input", "dynamic-argu-actor-address", "dynamic-argu-key-actor");
    actor.placeholder = "actor address";

    var typeName = el("select", "dynamic-argu-du-type", "dynamic-argu-key-du-type");
    Object.keys(schemas).forEach(function (schemaKey) {
      var option = document.createElement("option");
      option.value = schemaKey;
      option.textContent = schemaKey;
      typeName.appendChild(option);
    });

    var cases = el("div", "dynamic-argu-union-case-list", "dynamic-argu-key-union-cases");
    function renderCases() {
      cases.textContent = "";
      var schema = schemas[typeName.value];
      (schema ? schema.unionCases : []).forEach(function (unionCase) {
        var wrap = el("label", "dynamic-argu-union-case-check", "dynamic-argu-key-union-case-" + unionCase.name);
        var check = document.createElement("input");
        check.type = "checkbox";
        check.value = unionCase.name;
        check.checked = true;
        wrap.appendChild(check);
        wrap.appendChild(document.createTextNode(unionCase.name));
        cases.appendChild(wrap);
      });
    }
    typeName.addEventListener("change", renderCases);
    renderCases();

    var submit = el("button", "dynamic-argu-key-submit", "dynamic-argu-key-submit");
    submit.type = "button";
    submit.textContent = "Add target";
    submit.addEventListener("click", function () {
      var selectedCases = Array.from(cases.querySelectorAll("input:checked")).map(function (item) { return item.value; });
      ctx.submitKey({
        keys: [
          actor.value,
          "1:duType:" + typeName.value,
          "2:unionCases:" + selectedCases.join("|")
        ]
      });
    });

    root.appendChild(actor);
    root.appendChild(typeName);
    root.appendChild(cases);
    root.appendChild(submit);
    return root;
  });

  window.PulseTradeRegisterAppendInputRenderer("dynamic-argu-append-input", 100, function (ctx) {
    var typeName = String(ctx.duTypeName || "");
    var schema = schemas[typeName];
    if (!schema) return null;

    var allowedNames = unionCaseNamesFromContext(ctx, schema);
    var unionCases = schema.unionCases.filter(function (item) { return allowedNames.indexOf(item.name) >= 0; });
    if (unionCases.length === 0) return null;

    var root = el("div", "dynamic-argu-form", "dynamic-argu-form");
    root.setAttribute("data-dynamic-argu-du-type", typeName);
    root.setAttribute("data-dynamic-argu-union-cases", allowedNames.join(","));

    var selector = el("select", "dynamic-argu-union-case", "dynamic-argu-union-case");
    unionCases.forEach(function (unionCase) {
      var option = document.createElement("option");
      option.value = unionCase.name;
      option.textContent = unionCase.name;
      selector.appendChild(option);
    });

    var fields = el("div", "dynamic-argu-fields", "dynamic-argu-fields");
    var rawPreview = el("pre", "dynamic-argu-raw-preview", "dynamic-argu-raw-preview");
    var send = el("button", "dynamic-argu-send", "dynamic-argu-send");
    send.type = "button";
    send.textContent = "Send";

    function currentUnionCase() {
      return unionCases.filter(function (item) { return item.name === selector.value; })[0] || unionCases[0];
    }

    var refreshPreview = function () {
      rawPreview.textContent = buildRawArgu(currentUnionCase(), root);
    };

    var refreshFields = function () {
      fields.textContent = "";
      currentUnionCase().fields.forEach(function (field) { fields.appendChild(renderField(field)); });
      refreshPreview();
    };

    root.addEventListener("input", refreshPreview);
    root.addEventListener("change", refreshPreview);
    selector.addEventListener("change", refreshFields);
    send.addEventListener("click", function () {
      var raw = buildRawArgu(currentUnionCase(), root);
      rawPreview.textContent = raw;
      ctx.submit({ rawArgu: raw, duTypeName: typeName, unionCaseName: currentUnionCase().name });
    });

    root.appendChild(labelText("Union case"));
    root.appendChild(selector);
    root.appendChild(fields);
    root.appendChild(rawPreview);
    root.appendChild(send);
    refreshFields();
    return root;
  });
})();
""")
