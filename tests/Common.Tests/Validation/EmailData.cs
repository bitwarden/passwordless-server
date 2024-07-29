using Passwordless.Common.Validation;

namespace Passwordless.Common.Tests.Validation;

public static class EmailData
{
    public static IEnumerable<object[]> Valid =>
        new List<object[]>
        {
            new object[] { "\"Abc\\@def\"@example.com" },
            new object[] { "\"Fred Bloggs\"@example.com" },
            new object[] { "\"Joe\\\\Blow\"@example.com" },
            new object[] { "\"Abc@def\"@example.com" },
            new object[] { "customer/department=shipping@example.com" },
            new object[] { "$A12345@example.com" },
            new object[] { "!def!xyz%abc@example.com" },
            new object[] { "_somename@example.com" },
            new object[] { "valid@[1.1.1.1]" },
            new object[] { "valid.ipv4.addr@[123.1.72.10]" },
            new object[] { "valid.ipv4.addr@[255.255.255.255]" },
            new object[] { "valid.ipv6.addr@[IPv6:::]" },
            new object[] { "valid.ipv6.addr@[IPv6:0::1]" },
            new object[] { "valid.ipv6.addr@[IPv6:::12.34.56.78]" },
            new object[] { "valid.ipv6.addr@[IPv6:::3333:4444:5555:6666:7777:8888]" },
            new object[] { "valid.ipv6.addr@[IPv6:2607:f0d0:1002:51::4]" },
            new object[] { "valid.ipv6.addr@[IPv6:fe80::230:48ff:fe33:bc33]" },
            new object[] { "valid.ipv6.addr@[IPv6:fe80:0000:0000:0000:0202:b3ff:fe1e:8329]" },
            new object[] { "valid.ipv6v4.addr@[IPv6:::12.34.56.78]" },
            new object[] { "valid.ipv6v4.addr@[IPv6:aaaa:aaaa:aaaa:aaaa:aaaa:aaaa:127.0.0.1]" },
            new object[] { new string('a', 64) + "@example.com" }, // max local-part length (64 characters)
            new object[] { "valid@" + new string('a', 63) + ".com" }, // max subdomain length (64 characters)
            new object[]
            {
                "valid@" + new string('a', 60) + "." + new string('b', 60) + "." + new string('c', 60) + "." +
                new string('d', 61) + ".com"
            }, // max length (254 characters)
            new object[]
            {
                new string('a', 64) + "@" + new string('a', 45) + "." + new string('b', 46) + "." +
                new string('c', 45) + "." + new string('d', 46) + ".com"
            }, // max local-part length (64 characters)

            // examples from wikipedia
            new object[] { "niceandsimple@example.com" },
            new object[] { "very.common@example.com" },
            new object[] { "a.little.lengthy.but.fine@dept.example.com" },
            new object[] { "disposable.style.email.with+symbol@example.com" },
            new object[] { "user@[IPv6:2001:db8:1ff::a0b:dbd0]" },
            new object[] { "\"much.more unusual\"@example.com" },
            new object[] { "\"very.unusual.@.unusual.com\"@example.com" },
            new object[] { "\"very.( },:;<>[]\\\".VERY.\\\"very@\\\\ \\\"very\\\".unusual\"@strange.example.com" },
            new object[] { "postbox@com" },
            new object[] { "admin@mailserver1" },
            new object[] { "!#$%&'*+-/=?^_`{}|~@example.org" },
            new object[] { "\"()<>[]:,;@\\\\\\\"!#$%&'*+-/=?^_`{}| ~.a\"@example.org" },
            new object[] { "\" \"@example.org" },

            // examples from https://github.com/Sembiance/email-validator
            new object[] { "\"\\e\\s\\c\\a\\p\\e\\d\"@sld.com" },
            new object[] { "\"back\\slash\"@sld.com" },
            new object[] { "\"escaped\\\"quote\"@sld.com" },
            new object[] { "\"quoted\"@sld.com" },
            new object[] { "\"quoted-at-sign@sld.org\"@sld.com" },
            new object[] { "&'*+-./=?^_{}~@other-valid-characters-in-local.net" },
            new object[] { "01234567890@numbers-in-local.net" },
            new object[] { "a@single-character-in-local.org" },
            new object[] { "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@letters-in-local.org" },
            new object[] { "backticksarelegit@test.com" },
            new object[] { "bracketed-IP-instead-of-domain@[127.0.0.1]" },
            new object[] { "country-code-tld@sld.rw" },
            new object[] { "country-code-tld@sld.uk" },
            new object[] { "letters-in-sld@123.com" },
            new object[] { "local@dash-in-sld.com" },
            new object[] { "local@sld.newTLD" },
            new object[] { "local@sub.domains.com" },
            new object[] { "mixed-1234-in-{+^}-local@sld.net" },
            new object[] { "one-character-third-level@a.example.com" },
            new object[] { "one-letter-sld@x.org" },
            new object[] { "punycode-numbers-in-tld@sld.xn--3e0b707e" },
            new object[] { "single-character-in-sld@x.org" },
            new object[]
            {
                "the-character-limit@for-each-part.of-the-domain.is-sixty-three-characters.this-is-exactly-sixty-three-characters-so-it-is-valid-blah-blah.com"
            },
            new object[]
            {
                "the-total-length@of-an-entire-address.cannot-be-longer-than-two-hundred-and-fifty-six-characters.and-this-address-is-256-characters-exactly.so-it-should-be-valid.and-im-going-to-add-some-more-words-here.to-increase-the-length-blah-blah-blah-blah-blah.org"
            },
            new object[] { "uncommon-tld@sld.mobi" },
            new object[] { "uncommon-tld@sld.museum" },
            new object[] { "uncommon-tld@sld.travel" }
        };

    public static IEnumerable<object[]> ValidInternational =>
        new List<object[]>
        {
            new object[] { "伊昭傑@郵件.商務" }, // Chinese
            new object[] { "राम@मोहन.ईन्फो" }, // Hindi
            new object[] { "юзер@екзампл.ком" }, // Ukranian
            new object[] { "θσερ@εχαμπλε.ψομ" }, // Greek
            new object[]
            {
                "𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈𐍈@example.com"
            } // surrogate pair local-part
        };

    public static IEnumerable<object[]> Invalid =>
        new List<object[]>
            {
        new object[] { "", EmailValidationErrorCode.EmptyAddress, null, null },
        new object[] { "invalid", EmailValidationErrorCode.IncompleteLocalPart, 0, 7 },
        new object[] { "\"invalid\"", EmailValidationErrorCode.IncompleteLocalPart, 0, 9 },
        new object[] { "invalid@", EmailValidationErrorCode.IncompleteDomain, 8, 8 },
        new object[] { "invalid @", EmailValidationErrorCode.InvalidLocalPartCharacter, 7, 7 },
        new object[] { "invalid@[10]", EmailValidationErrorCode.InvalidIPAddress, 9, 9 },
        new object[] { "invalid@[10.1]", EmailValidationErrorCode.InvalidIPAddress, 9, 9 },
        new object[] { "invalid@[10.1.52]", EmailValidationErrorCode.InvalidIPAddress, 9, 16 },
        new object[] { "invalid@[256.256.256.256]", EmailValidationErrorCode.InvalidIPAddress, 9, 12 },
        new object[] { "invalid@[IPv6:123456]", EmailValidationErrorCode.InvalidIPAddress, 14, 20 },
        new object[] { "invalid@[127.0.0.1.]", EmailValidationErrorCode.UnterminatedIPAddressLiteral, 19, 19 },
        new object[] { "invalid@[127.0.0.1].", EmailValidationErrorCode.UnexpectedCharactersAfterDomain, 19,
            19 },
        new object[] { "invalid@[127.0.0.1]x", EmailValidationErrorCode.UnexpectedCharactersAfterDomain, 19,
            19 },
        new object[] { "invalid@domain1.com@domain2.com", EmailValidationErrorCode.None, null, null },
        new object[] { "\"locál-part\"@example.com", EmailValidationErrorCode.InvalidLocalPartCharacter, 4,
            4 }, // international local-part when allowInternational=false should fail
        new object[] { new string('a', 65) + "@example.com", EmailValidationErrorCode.LocalPartTooLong, 0,
            65 }, // local-part too long
        new object[] { "invalid@" + new string('a', 64) + ".com", EmailValidationErrorCode.DomainLabelTooLong,
            8, 72 }, // subdomain too long
        new object[] {
            "invalid@" + new string('a', 60) + "." + new string('b', 60) + "." + new string('c', 60) + "." +
            new string('d', 60) + ".com", EmailValidationErrorCode.AddressTooLong, null,
            null }, // too long (254 characters)
        new object[] { "invalid@[]", EmailValidationErrorCode.InvalidIPAddress, 9, 9 }, // empty IP literal
        new object[] { "invalid@[192.168.10", EmailValidationErrorCode.InvalidIPAddress, 9,
            19 }, // incomplete IPv4 literal
        new object[] { "invalid@[111.111.111.111", EmailValidationErrorCode.UnterminatedIPAddressLiteral, 24,
            24 }, // unenclosed IPv4 literal
        new object[] { "invalid@[IPv6:2607:f0d0:1002:51::4",
            EmailValidationErrorCode.UnterminatedIPAddressLiteral, 34, 34 }, // unenclosed IPv6 literal
        new object[] { "invalid@[IPv6:1111::1111::1111]", EmailValidationErrorCode.InvalidIPAddress, 14,
            26 }, // invalid IPv6-comp
        new object[] { "invalid@[IPv6:1111:::1111::1111]", EmailValidationErrorCode.InvalidIPAddress, 14,
            21 }, // more than 2 consecutive :'s in IPv6
        new object[] { "invalid@[IPv6:aaaa:aaaa:aaaa:aaaa:aaaa:aaaa:555.666.777.888]",
            EmailValidationErrorCode.InvalidIPAddress, 44, 47 }, // invalid IPv4 address in IPv6v4
        new object[] { "invalid@[IPv6:1111:1111]", EmailValidationErrorCode.InvalidIPAddress, 14,
            23 }, // incomplete IPv6
        new object[] { "invalid@[IPv6:1::2:]", EmailValidationErrorCode.InvalidIPAddress, 14,
            19 }, // incomplete IPv6
        new object[] { "invalid@[IPv6::1::1]", EmailValidationErrorCode.InvalidIPAddress, 14, 15 },
        new object[] { "\"invalid-qstring@example.com", EmailValidationErrorCode.UnterminatedQuotedString, 0,
            28 }, // unterminated q-string in local-part of the addr-spec
        new object[] { "\"control-\u007f-character\"@example.com",
            EmailValidationErrorCode.InvalidLocalPartCharacter, 9, 9 },
        new object[] { "\"control-\u001f-character\"@example.com",
            EmailValidationErrorCode.InvalidLocalPartCharacter, 9, 9 },
        new object[] { "\"control-\\\u007f-character\"@example.com",
            EmailValidationErrorCode.InvalidLocalPartCharacter, 10, 10 },

        // examples from Wikipedia
        new object[] { "Abc.example.com", EmailValidationErrorCode.IncompleteLocalPart, 0, 15 },
        new object[] { "A@b@c@example.com", EmailValidationErrorCode.InvalidDomainCharacter, 3, 3 },
        new object[] { "a\"b(c)d,e:f;g<h>i[j\\k]l@example.com",
            EmailValidationErrorCode.InvalidLocalPartCharacter, 1, 1 },
        new object[] { "just\"not\"right@example.com", EmailValidationErrorCode.InvalidLocalPartCharacter, 4,
            4 },
        new object[] { "this is\"not\\allowed@example.com", EmailValidationErrorCode.InvalidLocalPartCharacter,
            4, 4 },
        new object[] { "this\\ still\\\"not\\\\allowed@example.com",
            EmailValidationErrorCode.InvalidLocalPartCharacter, 4, 4 },

        // examples from https://github.com/Sembiance/email-validator
        new object[] { "! #$%`|@invalid-characters-in-local.org",
            EmailValidationErrorCode.InvalidLocalPartCharacter, 1, 1 },
        new object[] { "( },:;`|@more-invalid-characters-in-local.org",
            EmailValidationErrorCode.InvalidLocalPartCharacter, 0, 0 },
        new object[] { "* .local-starts-with-dot@sld.com", EmailValidationErrorCode.InvalidLocalPartCharacter,
            1, 1 },
        new object[] { "<>@[]`|@even-more-invalid-characters-in-local.org",
            EmailValidationErrorCode.InvalidLocalPartCharacter, 0, 0 },
        new object[] { "@missing-local.org", EmailValidationErrorCode.InvalidLocalPartCharacter, 0, 0 },
        new object[] { "IP-and-port@127.0.0.1:25", EmailValidationErrorCode.InvalidDomainCharacter, 21, 21 },
        new object[] { "another-invalid-ip@127.0.0.256", EmailValidationErrorCode.InvalidDomainCharacter, 30,
            30 },
        new object[] { "invalid", EmailValidationErrorCode.IncompleteLocalPart, 0, 7 },
        new object[] { "invalid-characters-in-sld@! \"#$%( },/;<>_[]`|.org",
            EmailValidationErrorCode.InvalidDomainCharacter, 26, 26 },
        new object[] { "invalid-ip@127.0.0.1.26", EmailValidationErrorCode.InvalidDomainCharacter, 23, 23 },
        new object[] { "local-ends-with-dot.@sld.com", EmailValidationErrorCode.InvalidLocalPartCharacter, 20,
            20 },
        new object[] { "missing-at-sign.net", EmailValidationErrorCode.IncompleteLocalPart, 0, 19 },
        new object[] { "missing-sld@.com", EmailValidationErrorCode.InvalidDomainCharacter, 12, 12 },
        new object[] { "missing-tld@sld.", EmailValidationErrorCode.IncompleteDomain, 12, 16 },
        new object[] { "sld-ends-with-dash@sld-.com", EmailValidationErrorCode.InvalidDomainCharacter, 22, 22 },
        new object[] { "sld-starts-with-dashsh@-sld.com", EmailValidationErrorCode.InvalidDomainCharacter, 23,
            23 },
        new object[] {
            "the-character-limit@for-each-part.of-the-domain.is-sixty-four-characters.this-subdomain-is-exactly-sixty-five-characters-so-it-is-invalid1.com",
            EmailValidationErrorCode.DomainLabelTooLong, 73, 138 },
        new object[] { "the-local-part-is-invalid-if-it-is-longer-than-sixty-four-characters@sld.net",
            EmailValidationErrorCode.LocalPartTooLong, 0, 68 },
        new object[] {
            "the-total-length@of-an-entire-address.cannot-be-longer-than-two-hundred-and-fifty-six-characters.and-this-address-is-257-characters-exactly.so-it-should-be-invalid.lets-add-some-extra-words-here.to-increase-the-length.beyond-the-256-character-limitation.org",
            EmailValidationErrorCode.AddressTooLong, null, null },
        new object[] { "two..consecutive-dots@sld.com", EmailValidationErrorCode.InvalidLocalPartCharacter, 4,
            4 },
        new object[] { "unbracketed-IP@127.0.0.1", EmailValidationErrorCode.IncompleteDomainLabel, 23, 24 },
        new object[] { "dot-first-in-domain@.test.de", EmailValidationErrorCode.InvalidDomainCharacter, 20,
            20 },
        new object[] { "single-character-tld@ns.i", EmailValidationErrorCode.IncompleteDomainLabel, 24, 25 },

        // examples of real (invalid) input from real users.
        new object[] { "No longer available.", EmailValidationErrorCode.InvalidLocalPartCharacter, 2, 2 },
        new object[] { "Moved.", EmailValidationErrorCode.IncompleteLocalPart, 0, 6 }
    };

    public static IEnumerable<object[]> InvalidInternational =>
    new List<object[]>
    {
        new object[] { "test@𐍈", EmailValidationErrorCode.IncompleteDomainLabel, 5, 7 }, // single "character" surrogate-pair domain
    };
}