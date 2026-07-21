import { SelectOption } from '../../shared/components/select/select.component';

/**
 * Option lists for the profile's country, time zone and language pickers.
 *
 * Everything here is derived from the browser's own Intl data rather than hard-coded
 * label tables. A checked-in list of country and time zone names is wrong the moment
 * either changes, is never updated, and cannot be localised — Intl is maintained by
 * the platform and renders names in the user's own locale for free.
 *
 * The server validates the same three fields independently, against `RegionInfo` and
 * `TimeZoneInfo`. These lists shape the UI; they are not the enforcement point.
 */

/**
 * ISO 3166-1 alpha-2 codes. This is the one list that must be stated explicitly —
 * there is no Intl API that enumerates regions — but only the codes are kept here.
 * The display names come from Intl, so this array never needs touching for a rename.
 */
const ISO_COUNTRY_CODES =
  'AD,AE,AF,AG,AI,AL,AM,AO,AQ,AR,AS,AT,AU,AW,AX,AZ,BA,BB,BD,BE,BF,BG,BH,BI,BJ,BL,BM,BN,BO,BQ,BR,BS,BT,BV,BW,BY,BZ,CA,CC,CD,CF,CG,CH,CI,CK,CL,CM,CN,CO,CR,CU,CV,CW,CX,CY,CZ,DE,DJ,DK,DM,DO,DZ,EC,EE,EG,EH,ER,ES,ET,FI,FJ,FK,FM,FO,FR,GA,GB,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GR,GS,GT,GU,GW,GY,HK,HM,HN,HR,HT,HU,ID,IE,IL,IM,IN,IO,IQ,IR,IS,IT,JE,JM,JO,JP,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LT,LU,LV,LY,MA,MC,MD,ME,MF,MG,MH,MK,ML,MM,MN,MO,MP,MQ,MR,MS,MT,MU,MV,MW,MX,MY,MZ,NA,NC,NE,NF,NG,NI,NL,NO,NP,NR,NU,NZ,OM,PA,PE,PF,PG,PH,PK,PL,PM,PN,PR,PS,PT,PW,PY,QA,RE,RO,RS,RU,RW,SA,SB,SC,SD,SE,SG,SH,SI,SJ,SK,SL,SM,SN,SO,SR,SS,ST,SV,SX,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TR,TT,TV,TW,TZ,UA,UG,UM,US,UY,UZ,VA,VC,VE,VG,VI,VN,VU,WF,WS,YE,YT,ZA,ZM,ZW'.split(
    ',',
  );

/** Language tags offered in the picker. Any valid BCP 47 tag is accepted by the server. */
const LANGUAGE_TAGS = [
  'en', 'en-GB', 'en-US', 'es', 'fr', 'de', 'it', 'pt', 'pt-BR',
  'nl', 'pl', 'ru', 'tr', 'ar', 'hi', 'bn', 'ta', 'te', 'zh-Hans', 'zh-Hant', 'ja', 'ko',
];

function displayNames(type: 'region' | 'language'): Intl.DisplayNames | null {
  try {
    return new Intl.DisplayNames(undefined, { type });
  } catch {
    // Older engines, or a locale with no data. Callers fall back to raw codes, which
    // are still selectable and still valid — degraded labels, working form.
    return null;
  }
}

function sortByLabel(options: SelectOption[]): SelectOption[] {
  return options.sort((a, b) => a.label.localeCompare(b.label));
}

export function countryOptions(): SelectOption[] {
  const names = displayNames('region');

  return sortByLabel(
    ISO_COUNTRY_CODES.map((code) => ({
      value: code,
      label: names?.of(code) ?? code,
    })),
  );
}

export function languageOptions(): SelectOption[] {
  const names = displayNames('language');

  return sortByLabel(
    LANGUAGE_TAGS.map((tag) => ({
      value: tag,
      label: names?.of(tag) ?? tag,
    })),
  );
}

/**
 * IANA time zone identifiers from the browser's own database.
 *
 * `Intl.supportedValuesOf` is not in older TypeScript lib definitions, hence the cast.
 * Where it is missing entirely, the list falls back to the single zone the browser
 * reports for the current machine — enough for the user to save a correct value.
 */
export function timeZoneOptions(): SelectOption[] {
  const intl = Intl as typeof Intl & { supportedValuesOf?: (key: string) => string[] };

  const zones = intl.supportedValuesOf
    ? intl.supportedValuesOf('timeZone')
    : [Intl.DateTimeFormat().resolvedOptions().timeZone].filter(Boolean);

  return zones.map((zone) => ({ value: zone, label: zone.replace(/_/g, ' ') }));
}

/** The browser's best guess, used to prefill an empty time zone rather than leaving it blank. */
export function detectedTimeZone(): string {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone ?? 'UTC';
  } catch {
    return 'UTC';
  }
}
