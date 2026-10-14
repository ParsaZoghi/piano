import { en } from '../i18n/en.js';
import { fa } from '../i18n/fa.js';
import { pitchClassName } from './notes.js';

const DICTIONARIES = { en, fa };
const RTL = new Set(['fa']);
export const LANGUAGES = [
    { id: 'en', label: 'English' },
    { id: 'fa', label: 'فارسی' },
];

let lang = 'en';
let digits = new Intl.NumberFormat('en', { useGrouping: false });

export function setLanguage(value) {
    lang = DICTIONARIES[value] ? value : 'en';
    digits = new Intl.NumberFormat(lang === 'fa' ? 'fa-IR' : 'en', { useGrouping: false });
    document.documentElement.lang = lang;
    document.documentElement.dir = RTL.has(lang) ? 'rtl' : 'ltr';
}

export const language = () => lang;

/** Numbers in the digits of the current language (۱۲۳ in Persian). */
export const num = (value) => digits.format(value);

const lookup = (dictionary, key) => key.split('.').reduce((node, part) => node?.[part], dictionary);

// {name} is filled from vars; {n:C} / {n:F#} becomes a note name in the
// learner's chosen naming (C or Do).
function fill(text, vars) {
    return text
        .replace(/\{n:([A-G]#?)\}/g, (_, token) => pitchClassName(token))
        .replace(/\{(\w+)\}/g, (match, name) => {
            if (!(name in vars)) return match;
            return typeof vars[name] === 'number' ? num(vars[name]) : vars[name];
        });
}

/** Translate a dotted key, falling back to English and then to the key. */
export function t(key, vars = {}) {
    const text = lookup(DICTIONARIES[lang], key) ?? lookup(en, key);
    return typeof text === 'string' ? fill(text, vars) : key;
}

/** Translate a key whose value is a list of strings. */
export function tList(key, vars = {}) {
    const list = lookup(DICTIONARIES[lang], key) ?? lookup(en, key);
    return Array.isArray(list) ? list.map((text) => fill(text, vars)) : [];
}

export const has = (key) => lookup(en, key) !== undefined;
