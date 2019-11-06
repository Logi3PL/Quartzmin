/// <reference path="webintellisense.ts"/>
/// <reference path="../../scripts/typings/codemirror/codemirror.d.ts" />
function ignore(x) {
}
/**
 * Simple data structure for providing a keyboard event to trigger the showing
 * of the DeclarationsIntellisense or MethodsIntellisense user interfaces.
 */
var CodeMirrorTrigger = (function () {
    function CodeMirrorTrigger() {
    }
    return CodeMirrorTrigger;
}());
/**
 * The triggers object
 */
var Triggers = (function () {
    function Triggers() {
    }
    return Triggers;
}());
/**
 * This class provides intellisense for either a textarea or an inputbox.
 */
var CodeMirrorIntellisense = (function () {
    function CodeMirrorIntellisense(editor) {
        var _this = this;
        this.decls = new wi.DeclarationsIntellisense();
        this.meths = new wi.MethodsIntellisense();
        this.triggers = { upDecls: [], downDecls: [], upMeths: [], downMeths: [] };
        this.declarationsCallback = ignore;
        this.methodsCallback = ignore;
        this.autoCompleteStart = { lineIndex: 0, columnIndex: 0 };
        this.hoverTimeout = 1000;
        this.tooltip = new wi.Tooltip();
        this.lastMouseX = 0;
        this.lastMouseY = 0;
        this.prevCoords = { line: 0, ch: 0 };
        this.editor = editor;
        // when the visiblity has changed for the declarations, set the position of the methods UI
        this.decls.onVisibleChanged(function (v) {
            if (v) {
                var coords = editor.cursorCoords(true, 'page');
                _this.decls.setPosition(coords.left, coords.bottom);
            }
        });
        // when the visiblity has changed for the methods, set the position of the methods UI
        this.meths.onVisibleChanged(function (v) {
            if (v) {
                var coords = editor.cursorCoords(true, 'page');
                _this.meths.setPosition(coords.left, coords.bottom);
            }
        });
        // when an item is chosen by the declarations UI, set the value.
        this.decls.onItemChosen(function (item) {
            var doc = editor.getDoc();
            var itemValue = item.value || item.name;
            var cursor = doc.getCursor();
            var line = doc.getLine(_this.autoCompleteStart.lineIndex);
            var startRange = { line: cursor.line, ch: _this.autoCompleteStart.columnIndex };
            var endRange = { line: cursor.line, ch: cursor.ch };
            doc.replaceRange(itemValue, startRange, endRange);
            doc.setSelection({ line: cursor.line, ch: cursor.ch + itemValue.length }, null);
            _this.decls.setVisible(false);
            editor.focus();
        });
        var timer = null;
        editor.getWrapperElement().addEventListener('mousemove', function (evt) {
            _this.lastMouseX = evt.clientX;
            _this.lastMouseY = evt.clientY;
            if (timer) {
                clearTimeout(timer);
            }
            timer = setTimeout(function () {
                _this.tooltip.setVisible(false);
                if (_this.hoverCallback) {
                    var source = editor.getDoc().getValue();
                    var coords = editor.coordsChar({ left: _this.lastMouseX, top: _this.lastMouseY });
                    if (coords.outside != true) {
                        if (_this.prevCoords.line !== coords.line || _this.prevCoords.ch !== coords.ch) {
                            _this.prevCoords = coords;
                            _this.hoverCallback(coords, evt);
                        }
                    }
                }
            }, _this.hoverTimeout);
        });
        CodeMirror.on(editor, 'cursorActivity', function (cm, evt) {
            _this.tooltip.setVisible(false);
        });
        CodeMirror.on(editor, 'keyup', function (cm, evt) {
            _this.tooltip.setVisible(false);
            if (_this.decls.isVisible()) {
                _this.decls.setFilter(_this.getFilterText());
            }
            if (!_this.processTriggers(_this.triggers.upDecls, evt, _this.declarationsCallback)) {
                _this.processTriggers(_this.triggers.upMeths, evt, _this.methodsCallback);
            }
        });
        CodeMirror.on(editor, 'keydown', function (cm, evt) {
            _this.tooltip.setVisible(false);
            if (_this.decls.isVisible()) {
                if (evt.keyCode === wi.Keys.Backspace) {
                    _this.decls.setFilter(_this.getFilterText());
                }
                else {
                    _this.decls.handleKeyDown(evt);
                }
            }
            if (!_this.processTriggers(_this.triggers.downDecls, evt, _this.declarationsCallback)) {
                _this.processTriggers(_this.triggers.downMeths, evt, _this.methodsCallback);
            }
            if (_this.meths.isVisible()) {
                _this.meths.handleKeyDown(evt);
            }
        });
    }
    CodeMirrorIntellisense.prototype.processTriggers = function (triggers, evt, callback) {
        var _this = this;
        triggers.forEach(function (item) {
            var doc = _this.editor.getDoc();
            var shiftKey = item.shiftKey || false;
            var ctrlKey = item.ctrlKey || false;
            var keyCode = item.keyCode || 0;
            var preventDefault = item.preventDefault || false;
            if (evt.keyCode === keyCode && evt.shiftKey === shiftKey && evt.ctrlKey === ctrlKey) {
                var cursor = doc.getCursor();
                _this.autoCompleteStart.columnIndex = cursor.ch;
                _this.autoCompleteStart.lineIndex = cursor.line;
                callback(item);
                if (preventDefault) {
                    evt.preventDefault();
                    evt.cancelBubble = true;
                }
                return true;
            }
        });
        return false;
    };
    /**
     * Adds a trigger
     * @param trigger The trigger to add
     * @param methsOrDecls The type (either Meths or Decls)
     */
    CodeMirrorIntellisense.prototype.addTrigger = function (trigger, methsOrDecls) {
        var type = trigger.type || 'up';
        if (this.triggers[type + methsOrDecls]) {
            this.triggers[type + methsOrDecls].push(trigger);
        }
    };
    /**
     * Gets the tooltip object
     */
    CodeMirrorIntellisense.prototype.getTooltip = function () {
        return this.tooltip;
    };
    /**
     * Shows a hover tooltip at the last position of the mouse
     * @param tooltipString The tooltip string to show
     */
    CodeMirrorIntellisense.prototype.showHoverTooltip = function (tooltipString) {
        if (tooltipString == null || tooltipString === '') {
            this.tooltip.setVisible(false);
        }
        else {
            var pos = this.editor.charCoords(this.prevCoords, '');
            this.tooltip.setText(tooltipString);
            this.tooltip.setPosition(this.lastMouseX, this.lastMouseY);
            this.tooltip.setVisible(true);
        }
    };
    /**
     * Adds a trigger to the list of triggers that can cause the declarations user interface
     * to popup.
     * @param trigger The trigger to add
     */
    CodeMirrorIntellisense.prototype.addDeclarationTrigger = function (trigger) {
        this.addTrigger(trigger, 'Decls');
    };
    /**
     * Adds a trigger to the list of triggers that can cause the methods user interface
     * to popup.
     * @param trigger The trigger to add
     */
    CodeMirrorIntellisense.prototype.addMethodsTrigger = function (trigger) {
        this.addTrigger(trigger, 'Meths');
    };
    /**
     * When the user hovers over some text, calls the specified function
     * @param callback The callback function
     */
    CodeMirrorIntellisense.prototype.onHover = function (callback) {
        this.hoverCallback = callback;
    };
    /**
     * Sets a callback to invoke when a key is pressed that causes the declarations list to
     * popup.
     * @param callback The callback to set
     */
    CodeMirrorIntellisense.prototype.onDeclaration = function (callback) {
        this.declarationsCallback = callback;
    };
    /**
     * Sets a callback to invoke when a key is pressed that causes the methods list to
     * popup.
     * @param callback The callback to set
     */
    CodeMirrorIntellisense.prototype.onMethod = function (callback) {
        this.methodsCallback = callback;
    };
    /**
     * Gets the text after startColumnIndex but before caret offset.
     */
    CodeMirrorIntellisense.prototype.getFilterText = function () {
        var doc = this.editor.getDoc();
        var cursor = doc.getCursor();
        var line = doc.getLine(this.autoCompleteStart.lineIndex);
        return line.substring(this.autoCompleteStart.columnIndex, cursor.ch);
    };
    /**
     * Gets the declarations user interface
     */
    CodeMirrorIntellisense.prototype.getDecls = function () {
        return this.decls;
    };
    /**
     * Gets the methods user interface
     */
    CodeMirrorIntellisense.prototype.getMeths = function () {
        return this.meths;
    };
    /**
     * Delegate for setting the methods to display to the user
     * @param data The methods to display
     */
    CodeMirrorIntellisense.prototype.setMethods = function (data) {
        this.meths.setMethods(data);
    };
    /**
     * Delegate for setting the declarations to display to the user
     * @param data - The declarations to display
     */
    CodeMirrorIntellisense.prototype.setDeclarations = function (data) {
        this.decls.setDeclarations(data);
    };
    /**
     * Sets the starting location where filtering can occur. This is set when
     * a trigger happens that would cause the declarations list to show
     * @param i The index to set
     */
    CodeMirrorIntellisense.prototype.setStartColumnIndex = function (i) {
        this.autoCompleteStart.columnIndex = i;
    };
    return CodeMirrorIntellisense;
}());
