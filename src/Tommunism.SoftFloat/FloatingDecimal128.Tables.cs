﻿#region Copyright
// Ported to C# by Thomas Kaiser (2022).
// Note that this is under different licensing terms than the rest of the
// SoftFloat library, because it was ported from the Ryu source code.
// Original C Source Code: https://github.com/ulfjack/ryu

// Copyright 2018 Ulf Adams
//
// The contents of this file may be used under the terms of the Apache License,
// Version 2.0.
//
//    (See accompanying file LICENSE-Apache or copy at
//     http://www.apache.org/licenses/LICENSE-2.0)
//
// Alternatively, the contents of this file may be used under the terms of
// the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE-Boost or copy at
//     https://www.boost.org/LICENSE_1_0.txt)
//
// Unless required by applicable law or agreed to in writing, this software
// is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.
#endregion

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tommunism.SoftFloat;

partial struct FloatingDecimal128
{
    private const int FLOAT_128_POW5_INV_BITCOUNT = 249;
    private const int FLOAT_128_POW5_BITCOUNT = 249;
    private const int POW5_TABLE_SIZE = 56;

    #region GENERIC_POW5_TABLE

    // There's no good way to define 128-bit constants in C#, so we use little-endian pairs of 64-bit constants.
    // Multidimensional tables are not great in C#, so just use a single table and make sure all indexes into it are multiplied by 2.
    private static readonly ulong[] _GENERIC_POW5_TABLE = new ulong[POW5_TABLE_SIZE * 2]
    {
                            1U,                    0U,
                            5U,                    0U,
                           25U,                    0U,
                          125U,                    0U,
                          625U,                    0U,
                         3125U,                    0U,
                        15625U,                    0U,
                        78125U,                    0U,
                       390625U,                    0U,
                      1953125U,                    0U,
                      9765625U,                    0U,
                     48828125U,                    0U,
                    244140625U,                    0U,
                   1220703125U,                    0U,
                   6103515625U,                    0U,
                  30517578125U,                    0U,
                 152587890625U,                    0U,
                 762939453125U,                    0U,
                3814697265625U,                    0U,
               19073486328125U,                    0U,
               95367431640625U,                    0U,
              476837158203125U,                    0U,
             2384185791015625U,                    0U,
            11920928955078125U,                    0U,
            59604644775390625U,                    0U,
           298023223876953125U,                    0U,
          1490116119384765625U,                    0U,
          7450580596923828125U,                    0U,
           359414837200037393U,                    2U,
          1797074186000186965U,                   10U,
          8985370930000934825U,                   50U,
          8033366502585570893U,                  252U,
          3273344365508751233U,                 1262U,
         16366721827543756165U,                 6310U,
          8046632842880574361U,                31554U,
          3339676066983768573U,               157772U,
         16698380334918842865U,               788860U,
          9704925379756007861U,              3944304U,
         11631138751360936073U,             19721522U,
          2815461535676025517U,             98607613U,
         14077307678380127585U,            493038065U,
         15046306170771983077U,           2465190328U,
          1444554559021708921U,          12325951644U,
          7222772795108544605U,          61629758220U,
         17667119901833171409U,         308148791101U,
         14548623214327650581U,        1540743955509U,
         17402883850509598057U,        7703719777548U,
         13227442957709783821U,       38518598887744U,
         10796982567420264257U,      192592994438723U,
         17091424689682218053U,      962964972193617U,
         11670147153572883801U,     4814824860968089U,
          3010503546735764157U,    24074124304840448U,
         15052517733678820785U,   120370621524202240U,
          1475612373555897461U,   601853107621011204U,
          7378061867779487305U,  3009265538105056020U,
         18443565265187884909U, 15046327690525280101U
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 GENERIC_POW5_TABLE(int index)
    {
        Debug.Assert(index is >= 0 and < POW5_TABLE_SIZE);
        ref ulong entry = ref _GENERIC_POW5_TABLE[index * 2]; // safer than below in case assertion fails
        //Unsafe.AddByteOffset(ref MemoryMarshal.GetArrayDataReference(_GENERIC_POW5_TABLE), index * sizeof(ulong) * 2);
        return new(Unsafe.AddByteOffset(ref entry, sizeof(ulong)), entry);
    }

    #endregion

    #region GENERIC_POW5_SPLIT

    private const int GENERIC_POW5_SPLIT_SIZE = 89;

    // There's no way to define 256-bit constants in C#, so we use groups of 64-bit constants.
    // Multidimensional tables are not great in C#, so just use a single table and make sure all indexes into it are multiplied by 4.
    private static readonly ulong[] _GENERIC_POW5_SPLIT = new ulong[GENERIC_POW5_SPLIT_SIZE * 4]
    {
                           0U,                    0U,                    0U,    72057594037927936U,
                           0U,  5206161169240293376U,  4575641699882439235U,    73468396926392969U,
         3360510775605221349U,  6983200512169538081U,  4325643253124434363U,    74906821675075173U,
        11917660854915489451U,  9652941469841108803U,   946308467778435600U,    76373409087490117U,
         1994853395185689235U, 16102657350889591545U,  6847013871814915412U,    77868710555449746U,
          958415760277438274U, 15059347134713823592U,  7329070255463483331U,    79393288266368765U,
         2065144883315240188U,  7145278325844925976U, 14718454754511147343U,    80947715414629833U,
         8980391188862868935U, 13709057401304208685U,  8230434828742694591U,    82532576417087045U,
          432148644612782575U,  7960151582448466064U, 12056089168559840552U,    84148467132788711U,
          484109300864744403U, 15010663910730448582U, 16824949663447227068U,    85795995087002057U,
        14793711725276144220U, 16494403799991899904U, 10145107106505865967U,    87475779699624060U,
        15427548291869817042U, 12330588654550505203U, 13980791795114552342U,    89188452518064298U,
         9979404135116626552U, 13477446383271537499U, 14459862802511591337U,    90934657454687378U,
        12385121150303452775U,  9097130814231585614U,  6523855782339765207U,    92715051028904201U,
         1822931022538209743U, 16062974719797586441U,  3619180286173516788U,    94530302614003091U,
        12318611738248470829U, 13330752208259324507U, 10986694768744162601U,    96381094688813589U,
        13684493829640282333U,  7674802078297225834U, 15208116197624593182U,    98268123094297527U,
         5408877057066295332U,  6470124174091971006U, 15112713923117703147U,   100192097295163851U,
        11407083166564425062U, 18189998238742408185U,  4337638702446708282U,   102153740646605557U,
         4112405898036935485U,   924624216579956435U, 14251108172073737125U,   104153790666259019U,
        16996739107011444789U, 10015944118339042475U,  2395188869672266257U,   106192999311487969U,
         4588314690421337879U,  5339991768263654604U, 15441007590670620066U,   108272133262096356U,
         2286159977890359825U, 14329706763185060248U,  5980012964059367667U,   110391974208576409U,
         9654767503237031099U, 11293544302844823188U, 11739932712678287805U,   112553319146000238U,
        11362964448496095896U,  7990659682315657680U,   251480263940996374U,   114756980673665505U,
         1423410421096377129U, 14274395557581462179U, 16553482793602208894U,   117003787300607788U,
         2070444190619093137U, 11517140404712147401U, 11657844572835578076U,   119294583757094535U,
         7648316884775828921U, 15264332483297977688U,   247182277434709002U,   121630231312217685U,
        17410896758132241352U, 10923914482914417070U, 13976383996795783649U,   124011608097704390U,
         9542674537907272703U,  3079432708831728956U, 14235189590642919676U,   126439609438067572U,
        10364666969937261816U,  8464573184892924210U, 12758646866025101190U,   128915148187220428U,
        14720354822146013883U, 11480204489231511423U,  7449876034836187038U,   131439155071681461U,
         1692907053653558553U, 17835392458598425233U,  1754856712536736598U,   134012579040499057U,
         5620591334531458755U, 11361776175667106627U, 13350215315297937856U,   136636387622027174U,
        17455759733928092601U, 10362573084069962561U, 11246018728801810510U,   139311567287686283U,
         2465404073814044982U, 17694822665274381860U,  1509954037718722697U,   142039123822846312U,
         2152236053329638369U, 11202280800589637091U, 16388426812920420176U,    72410041352485523U,
        17319024055671609028U, 10944982848661280484U,  2457150158022562661U,    73827744744583080U,
        17511219308535248024U,  5122059497846768077U,  2089605804219668451U,    75273205100637900U,
        10082673333144031533U, 14429008783411894887U, 12842832230171903890U,    76746965869337783U,
        16196653406315961184U, 10260180891682904501U, 10537411930446752461U,    78249581139456266U,
        15084422041749743389U,   234835370106753111U, 16662517110286225617U,    79781615848172976U,
         8199644021067702606U,  3787318116274991885U,  7438130039325743106U,    81343645993472659U,
        12039493937039359765U,  9773822153580393709U,  5945428874398357806U,    82936258850702722U,
          984543865091303961U,  7975107621689454830U,  6556665988501773347U,    84560053193370726U,
         9633317878125234244U, 16099592426808915028U,  9706674539190598200U,    86215639518264828U,
         6860695058870476186U,  4471839111886709592U,  7828342285492709568U,    87903640274981819U,
        14583324717644598331U,  4496120889473451238U,  5290040788305728466U,    89624690099949049U,
        18093669366515003715U, 12879506572606942994U, 18005739787089675377U,    91379436055028227U,
        17997493966862379937U, 14646222655265145582U, 10265023312844161858U,    93168537870790806U,
        12283848109039722318U, 11290258077250314935U,  9878160025624946825U,    94992668194556404U,
         8087752761883078164U,  5262596608437575693U, 11093553063763274413U,    96852512843287537U,
        15027787746776840781U, 12250273651168257752U,  9290470558712181914U,    98748771061435726U,
        15003915578366724489U,  2937334162439764327U,  5404085603526796602U,   100682155783835929U,
         5225610465224746757U, 14932114897406142027U,  2774647558180708010U,   102653393903748137U,
        17112957703385190360U, 12069082008339002412U,  3901112447086388439U,   104663226546146909U,
         4062324464323300238U,  3992768146772240329U, 15757196565593695724U,   106712409346361594U,
         5525364615810306701U, 11855206026704935156U, 11344868740897365300U,   108801712734172003U,
         9274143661888462646U,  4478365862348432381U, 18010077872551661771U,   110931922223466333U,
        12604141221930060148U,  8930937759942591500U,  9382183116147201338U,   113103838707570263U,
        14513929377491886653U,  1410646149696279084U,   587092196850797612U,   115318278760358235U,
         2226851524999454362U,  7717102471110805679U,  7187441550995571734U,   117576074943260147U,
         5527526061344932763U,  2347100676188369132U, 16976241418824030445U,   119878076118278875U,
         6088479778147221611U, 17669593130014777580U, 10991124207197663546U,   122225147767136307U,
        11107734086759692041U,  3391795220306863431U, 17233960908859089158U,   124618172316667879U,
         7913172514655155198U, 17726879005381242552U,   641069866244011540U,   127058049470587962U,
        12596991768458713949U, 15714785522479904446U,  6035972567136116512U,   129545696547750811U,
        16901996933781815980U,  4275085211437148707U, 14091642539965169063U,   132082048827034281U,
         7524574627987869240U, 15661204384239316051U,  2444526454225712267U,   134668059898975949U,
         8199251625090479942U,  6803282222165044067U, 16064817666437851504U,   137304702024293857U,
         4453256673338111920U, 15269922543084434181U,  3139961729834750852U,   139992966499426682U,
        15841763546372731299U,  3013174075437671812U,  4383755396295695606U,   142733864029230733U,
         9771896230907310329U,  4900659362437687569U, 12386126719044266361U,    72764212553486967U,
         9420455527449565190U,  1859606122611023693U,  6555040298902684281U,    74188850200884818U,
         5146105983135678095U,  2287300449992174951U,  4325371679080264751U,    75641380576797959U,
        11019359372592553360U,  8422686425957443718U,  7175176077944048210U,    77122349788024458U,
        11005742969399620716U,  4132174559240043701U,  9372258443096612118U,    78632314633490790U,
         8887589641394725840U,  8029899502466543662U, 14582206497241572853U,    80171842813591127U,
          360247523705545899U, 12568341805293354211U, 14653258284762517866U,    81741513143625247U,
        12314272731984275834U,  4740745023227177044U,  6141631472368337539U,    83341915771415304U,
          441052047733984759U,  7940090120939869826U, 11750200619921094248U,    84973652399183278U,
         3436657868127012749U,  9187006432149937667U, 16389726097323041290U,    86637336509772529U,
        13490220260784534044U, 15339072891382896702U,  8846102360835316895U,    88333593597298497U,
         4125672032094859833U,   158347675704003277U, 10592598512749774447U,    90063061402315272U,
        12189928252974395775U,  2386931199439295891U,  7009030566469913276U,    91826390151586454U,
         9256479608339282969U,  2844900158963599229U, 11148388908923225596U,    93624242802550437U,
        11584393507658707408U,  2863659090805147914U,  9873421561981063551U,    95457295292572042U,
        13984297296943171390U,  1931468383973130608U, 12905719743235082319U,    97326236793074198U,
         5837045222254987499U, 10213498696735864176U, 14893951506257020749U,    99231769968645227U
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt256M GENERIC_POW5_SPLIT(int index)
    {
        Debug.Assert(index is >= 0 and < GENERIC_POW5_SPLIT_SIZE);
        ref ulong entry = ref _GENERIC_POW5_SPLIT[index * 4]; // safer than below in case assertion fails
        //Unsafe.AddByteOffset(ref MemoryMarshal.GetArrayDataReference(_GENERIC_POW5_SPLIT), index * sizeof(ulong) * 4);
        return new(
            v192: Unsafe.AddByteOffset(ref entry, sizeof(ulong) * 3),
            v128: Unsafe.AddByteOffset(ref entry, sizeof(ulong) * 2),
            v64: Unsafe.AddByteOffset(ref entry, sizeof(ulong) * 1),
            v0: entry
        );
    }

    #endregion

    #region POW5_ERRORS

    // Unfortunately, the results are sometimes off by one or two. We use an additional
    // lookup table to store those cases and adjust the result.

    private const int POW5_ERRORS_SIZE = 156;

    private static readonly ulong[] POW5_ERRORS = new ulong[POW5_ERRORS_SIZE]
    {
        0x0000000000000000U, 0x0000000000000000U, 0x0000000000000000U, 0x9555596400000000U,
        0x65a6569525565555U, 0x4415551445449655U, 0x5105015504144541U, 0x65a69969a6965964U,
        0x5054955969959656U, 0x5105154515554145U, 0x4055511051591555U, 0x5500514455550115U,
        0x0041140014145515U, 0x1005440545511051U, 0x0014405450411004U, 0x0414440010500000U,
        0x0044000440010040U, 0x5551155000004001U, 0x4554555454544114U, 0x5150045544005441U,
        0x0001111400054501U, 0x6550955555554554U, 0x1504159645559559U, 0x4105055141454545U,
        0x1411541410405454U, 0x0415555044545555U, 0x0014154115405550U, 0x1540055040411445U,
        0x0000000500000000U, 0x5644000000000000U, 0x1155555591596555U, 0x0410440054569565U,
        0x5145100010010005U, 0x0555041405500150U, 0x4141450455140450U, 0x0000000144000140U,
        0x5114004001105410U, 0x4444100404005504U, 0x0414014410001015U, 0x5145055155555015U,
        0x0141041444445540U, 0x0000100451541414U, 0x4105041104155550U, 0x0500501150451145U,
        0x1001050000004114U, 0x5551504400141045U, 0x5110545410151454U, 0x0100001400004040U,
        0x5040010111040000U, 0x0140000150541100U, 0x4400140400104110U, 0x5011014405545004U,
        0x0000000044155440U, 0x0000000010000000U, 0x1100401444440001U, 0x0040401010055111U,
        0x5155155551405454U, 0x0444440015514411U, 0x0054505054014101U, 0x0451015441115511U,
        0x1541411401140551U, 0x4155104514445110U, 0x4141145450145515U, 0x5451445055155050U,
        0x4400515554110054U, 0x5111145104501151U, 0x565a655455500501U, 0x5565555555525955U,
        0x0550511500405695U, 0x4415504051054544U, 0x6555595965555554U, 0x0100915915555655U,
        0x5540001510001001U, 0x5450051414000544U, 0x1405010555555551U, 0x5555515555644155U,
        0x5555055595496555U, 0x5451045004415000U, 0x5450510144040144U, 0x5554155555556455U,
        0x5051555495415555U, 0x5555554555555545U, 0x0000000010005455U, 0x4000005000040000U,
        0x5565555555555954U, 0x5554559555555505U, 0x9645545495552555U, 0x4000400055955564U,
        0x0040000000000001U, 0x4004100100000000U, 0x5540040440000411U, 0x4565555955545644U,
        0x1140659549651556U, 0x0100000410010000U, 0x5555515400004001U, 0x5955545555155255U,
        0x5151055545505556U, 0x5051454510554515U, 0x0501500050415554U, 0x5044154005441005U,
        0x1455445450550455U, 0x0010144055144545U, 0x0000401100000004U, 0x1050145050000010U,
        0x0415004554011540U, 0x1000510100151150U, 0x0100040400001144U, 0x0000000000000000U,
        0x0550004400000100U, 0x0151145041451151U, 0x0000400400005450U, 0x0000100044010004U,
        0x0100054100050040U, 0x0504400005410010U, 0x4011410445500105U, 0x0000404000144411U,
        0x0101504404500000U, 0x0000005044400400U, 0x0000000014000100U, 0x0404440414000000U,
        0x5554100410000140U, 0x4555455544505555U, 0x5454105055455455U, 0x0115454155454015U,
        0x4404110000045100U, 0x4400001100101501U, 0x6596955956966a94U, 0x0040655955665965U,
        0x5554144400100155U, 0xa549495401011041U, 0x5596555565955555U, 0x5569965959549555U,
        0x969565a655555456U, 0x0000001000000000U, 0x0000000040000140U, 0x0000040100000000U,
        0x1415454400000000U, 0x5410415411454114U, 0x0400040104000154U, 0x0504045000000411U,
        0x0000001000000010U, 0x5554000000001040U, 0x5549155551556595U, 0x1455541055515555U,
        0x0510555454554541U, 0x9555555555540455U, 0x6455456555556465U, 0x4524565555654514U,
        0x5554655255559545U, 0x9555455441155556U, 0x0000000051515555U, 0x0010005040000550U,
        0x5044044040000000U, 0x1045040440010500U, 0x0000400000040000U, 0x0000000000000000U
    };

    #endregion

    #region GENERIC_POW5_INV_SPLIT

    private const int GENERIC_POW5_INV_SPLIT_SIZE = 89;

    // There's no way to define 256-bit constants in C#, so we use groups of 64-bit constants.
    // Multidimensional tables are not great in C#, so just use a single table and make sure all indexes into it are multiplied by 4.
    private static readonly ulong[] _GENERIC_POW5_INV_SPLIT = new ulong[GENERIC_POW5_INV_SPLIT_SIZE * 4]
    {
                           0U,                    0U,                    0U,   144115188075855872U,
         1573859546583440065U,  2691002611772552616U,  6763753280790178510U,   141347765182270746U,
        12960290449513840412U, 12345512957918226762U, 18057899791198622765U,   138633484706040742U,
         7615871757716765416U,  9507132263365501332U,  4879801712092008245U,   135971326161092377U,
         7869961150745287587U,  5804035291554591636U,  8883897266325833928U,   133360288657597085U,
         2942118023529634767U, 15128191429820565086U, 10638459445243230718U,   130799390525667397U,
        14188759758411913794U,  5362791266439207815U,  8068821289119264054U,   128287668946279217U,
         7183196927902545212U,  1952291723540117099U, 12075928209936341512U,   125824179589281448U,
         5672588001402349748U, 17892323620748423487U,  9874578446960390364U,   123407996258356868U,
         4442590541217566325U,  4558254706293456445U, 10343828952663182727U,   121038210542800766U,
         3005560928406962566U,  2082271027139057888U, 13961184524927245081U,   118713931475986426U,
        13299058168408384786U, 17834349496131278595U,  9029906103900731664U,   116434285200389047U,
         5414878118283973035U, 13079825470227392078U, 17897304791683760280U,   114198414639042157U,
        14609755883382484834U, 14991702445765844156U,  3269802549772755411U,   112005479173303009U,
        15967774957605076027U,  2511532636717499923U, 16221038267832563171U,   109854654326805788U,
         9269330061621627145U,  3332501053426257392U, 16223281189403734630U,   107745131455483836U,
        16739559299223642282U,  1873986623300664530U,  6546709159471442872U,   105676117443544318U,
        17116435360051202055U,  1359075105581853924U,  2038341371621886470U,   103646834405281051U,
        17144715798009627550U,  3201623802661132408U,  9757551605154622431U,   101656519392613377U,
        17580479792687825857U,  6546633380567327312U, 15099972427870912398U,    99704424108241124U,
         9726477118325522902U, 14578369026754005435U, 11728055595254428803U,    97789814624307808U,
          134593949518343635U,  5715151379816901985U,  1660163707976377376U,    95911971106466306U,
         5515914027713859358U,  7124354893273815720U,  5548463282858794077U,    94070187543243255U,
         6188403395862945512U,  5681264392632320838U, 15417410852121406654U,    92263771480600430U,
        15908890877468271457U, 10398888261125597540U,  4817794962769172309U,    90492043761593298U,
         1413077535082201005U, 12675058125384151580U,  7731426132303759597U,    88754338271028867U,
         1486733163972670293U, 11369385300195092554U, 11610016711694864110U,    87050001685026843U,
         8788596583757589684U,  3978580923851924802U,  9255162428306775812U,    85378393225389919U,
         7203518319660962120U, 15044736224407683725U,  2488132019818199792U,    83738884418690858U,
         4004175967662388707U, 18236988667757575407U, 15613100370957482671U,    82130858859985791U,
        18371903370586036463U,    53497579022921640U, 16465963977267203307U,    80553711981064899U,
        10170778323887491315U,  1999668801648976001U, 10209763593579456445U,    79006850823153334U,
        17108131712433974546U, 16825784443029944237U,  2078700786753338945U,    77489693813976938U,
        17221789422665858532U, 12145427517550446164U,  5391414622238668005U,    76001670549108934U,
         4859588996898795878U,  1715798948121313204U,  3950858167455137171U,    74542221577515387U,
        13513469241795711526U,   631367850494860526U, 10517278915021816160U,    73110798191218799U,
        11757513142672073111U,  2581974932255022228U, 17498959383193606459U,   143413724438001539U,
        14524355192525042817U,  5640643347559376447U,  1309659274756813016U,   140659771648132296U,
         2765095348461978538U, 11021111021896007722U,  3224303603779962366U,   137958702611185230U,
        12373410389187981037U, 13679193545685856195U, 11644609038462631561U,   135309501808182158U,
        12813176257562780151U,  3754199046160268020U,  9954691079802960722U,   132711173221007413U,
        17557452279667723458U,  3237799193992485824U, 17893947919029030695U,   130162739957935629U,
        14634200999559435155U,  4123869946105211004U,  6955301747350769239U,   127663243886350468U,
         2185352760627740240U,  2864813346878886844U, 13049218671329690184U,   125211745272516185U,
         6143438674322183002U, 10464733336980678750U,  6982925169933978309U,   122807322428266620U,
         1099509117817174576U, 10202656147550524081U,   754997032816608484U,   120449071364478757U,
         2410631293559367023U, 17407273750261453804U, 15307291918933463037U,   118136105451200587U,
        12224968375134586697U,  1664436604907828062U, 11506086230137787358U,   115867555084305488U,
         3495926216898000888U, 18392536965197424288U, 10992889188570643156U,   113642567358547782U,
         8744506286256259680U,  3966568369496879937U, 18342264969761820037U,   111460305746896569U,
         7689600520560455039U,  5254331190877624630U,  9628558080573245556U,   109319949786027263U,
        11862637625618819436U,  3456120362318976488U, 14690471063106001082U,   107220694767852583U,
         5697330450030126444U, 12424082405392918899U,   358204170751754904U,   105161751436977040U,
        11257457505097373622U, 15373192700214208870U,   671619062372033814U,   103142345693961148U,
        16850355018477166700U,  1913910419361963966U,  4550257919755970531U,   101161718304283822U,
         9670835567561997011U, 10584031339132130638U,  3060560222974851757U,    99219124612893520U,
         7698686577353054710U, 11689292838639130817U, 11806331021588878241U,    97313834264240819U,
        12233569599615692137U,  3347791226108469959U, 10333904326094451110U,    95445130927687169U,
        13049400362825383933U, 17142621313007799680U,  3790542585289224168U,    93612312028186576U,
        12430457242474442072U,  5625077542189557960U, 14765055286236672238U,    91814688482138969U,
         4759444137752473128U,  2230562561567025078U,  4954443037339580076U,    90051584438315940U,
         7246913525170274758U,  8910297835195760709U,  4015904029508858381U,    88322337023761438U,
        12854430245836432067U,  8135139748065431455U, 11548083631386317976U,    86626296094571907U,
         4848827254502687803U,  4789491250196085625U,  3988192420450664125U,    84962823991462151U,
         7435538409611286684U,   904061756819742353U, 14598026519493048444U,    83331295300025028U,
        11042616160352530997U,  8948390828345326218U, 10052651191118271927U,    81731096615594853U,
        11059348291563778943U, 11696515766184685544U,  3783210511290897367U,    80161626312626082U,
         7020010856491885826U,  5025093219346041680U,  8960210401638911765U,    78622294318500592U,
        17732844474490699984U,  7820866704994446502U,  6088373186798844243U,    77112521891678506U,
          688278527545590501U,  3045610706602776618U,  8684243536999567610U,    75631741404109150U,
         2734573255120657297U,  3903146411440697663U,  9470794821691856713U,    74179396127820347U,
        15996457521023071259U,  4776627823451271680U, 12394856457265744744U,    72754940025605801U,
        13492065758834518331U,  7390517611012222399U,  1630485387832860230U,   142715675091463768U,
        13665021627282055864U,  9897834675523659302U, 17907668136755296849U,   139975126841173266U,
         9603773719399446181U, 10771916301484339398U, 10672699855989487527U,   137287204938390542U,
         3630218541553511265U,  8139010004241080614U,  2876479648932814543U,   134650898807055963U,
         8318835909686377084U,  9525369258927993371U,  2796120270400437057U,   132065217277054270U,
        11190003059043290163U, 12424345635599592110U, 12539346395388933763U,   129529188211565064U,
         8701968833973242276U,   820569587086330727U,  2315591597351480110U,   127041858141569228U,
         5115113890115690487U, 16906305245394587826U,  9899749468931071388U,   124602291907373862U,
        15543535488939245974U, 10945189844466391399U,  3553863472349432246U,   122209572307020975U,
         7709257252608325038U,  1191832167690640880U, 15077137020234258537U,   119862799751447719U,
         7541333244210021737U,  9790054727902174575U,  5160944773155322014U,   117561091926268545U,
        12297384708782857832U,  1281328873123467374U,  4827925254630475769U,   115303583460052092U,
        13243237906232367265U, 15873887428139547641U,  3607993172301799599U,   113089425598968120U,
        11384616453739611114U, 15184114243769211033U, 13148448124803481057U,   110917785887682141U,
        17727970963596660683U,  1196965221832671990U, 14537830463956404138U,   108787847856377790U,
        17241367586707330931U,  8880584684128262874U, 11173506540726547818U,   106698810713789254U,
         7184427196661305643U, 14332510582433188173U, 14230167953789677901U,   104649889046128358U
    };

    // NOTE: Returning a ref ulong would probably be faster (but not as safe).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt256M GENERIC_POW5_INV_SPLIT(int index)
    {
        Debug.Assert(index is >= 0 and < GENERIC_POW5_INV_SPLIT_SIZE);
        ref ulong entry = ref _GENERIC_POW5_INV_SPLIT[index * 4]; // safer than below in case assertion fails
        //Unsafe.AddByteOffset(ref MemoryMarshal.GetArrayDataReference(_GENERIC_POW5_INV_SPLIT), index * sizeof(ulong) * 4);
        return new(
            v192: Unsafe.AddByteOffset(ref entry, sizeof(ulong) * 3),
            v128: Unsafe.AddByteOffset(ref entry, sizeof(ulong) * 2),
            v64: Unsafe.AddByteOffset(ref entry, sizeof(ulong) * 1),
            v0: entry
        );
    }

    #endregion

    #region POW5_INV_ERRORS

    private const int POW5_INV_ERRORS_SIZE = 154;

    private static readonly ulong[] POW5_INV_ERRORS = new ulong[POW5_INV_ERRORS_SIZE]
    {
        0x1144155514145504U, 0x0000541555401141U, 0x0000000000000000U, 0x0154454000000000U,
        0x4114105515544440U, 0x0001001111500415U, 0x4041411410011000U, 0x5550114515155014U,
        0x1404100041554551U, 0x0515000450404410U, 0x5054544401140004U, 0x5155501005555105U,
        0x1144141000105515U, 0x0541500000500000U, 0x1104105540444140U, 0x4000015055514110U,
        0x0054010450004005U, 0x4155515404100005U, 0x5155145045155555U, 0x1511555515440558U,
        0x5558544555515555U, 0x0000000000000010U, 0x5004000000000050U, 0x1415510100000010U,
        0x4545555444514500U, 0x5155151555555551U, 0x1441540144044554U, 0x5150104045544400U,
        0x5450545401444040U, 0x5554455045501400U, 0x4655155555555145U, 0x1000010055455055U,
        0x1000004000055004U, 0x4455405104000005U, 0x4500114504150545U, 0x0000000014000000U,
        0x5450000000000000U, 0x5514551511445555U, 0x4111501040555451U, 0x4515445500054444U,
        0x5101500104100441U, 0x1545115155545055U, 0x0000000000000000U, 0x1554000000100000U,
        0x5555545595551555U, 0x5555051851455955U, 0x5555555555555559U, 0x0000400011001555U,
        0x0000004400040000U, 0x5455511555554554U, 0x5614555544115445U, 0x6455156145555155U,
        0x5455855455415455U, 0x5515555144555545U, 0x0114400000145155U, 0x0000051000450511U,
        0x4455154554445100U, 0x4554150141544455U, 0x65955555559a5965U, 0x5555555854559559U,
        0x9569654559616595U, 0x1040044040005565U, 0x1010010500011044U, 0x1554015545154540U,
        0x4440555401545441U, 0x1014441450550105U, 0x4545400410504145U, 0x5015111541040151U,
        0x5145051154000410U, 0x1040001044545044U, 0x4001400000151410U, 0x0540000044040000U,
        0x0510555454411544U, 0x0400054054141550U, 0x1001041145001100U, 0x0000000140000000U,
        0x0000000014100000U, 0x1544005454000140U, 0x4050055505445145U, 0x0011511104504155U,
        0x5505544415045055U, 0x1155154445515554U, 0x0000000000004555U, 0x0000000000000000U,
        0x5101010510400004U, 0x1514045044440400U, 0x5515519555515555U, 0x4554545441555545U,
        0x1551055955551515U, 0x0150000011505515U, 0x0044005040400000U, 0x0004001004010050U,
        0x0000051004450414U, 0x0114001101001144U, 0x0401000001000001U, 0x4500010001000401U,
        0x0004100000005000U, 0x0105000441101100U, 0x0455455550454540U, 0x5404050144105505U,
        0x4101510540555455U, 0x1055541411451555U, 0x5451445110115505U, 0x1154110010101545U,
        0x1145140450054055U, 0x5555565415551554U, 0x1550559555555555U, 0x5555541545045141U,
        0x4555455450500100U, 0x5510454545554555U, 0x1510140115045455U, 0x1001050040111510U,
        0x5555454555555504U, 0x9954155545515554U, 0x6596656555555555U, 0x0140410051555559U,
        0x0011104010001544U, 0x965669659a680501U, 0x5655a55955556955U, 0x4015111014404514U,
        0x1414155554505145U, 0x0540040011051404U, 0x1010000000015005U, 0x0010054050004410U,
        0x5041104014000100U, 0x4440010500100001U, 0x1155510504545554U, 0x0450151545115541U,
        0x4000100400110440U, 0x1004440010514440U, 0x0000115050450000U, 0x0545404455541500U,
        0x1051051555505101U, 0x5505144554544144U, 0x4550545555515550U, 0x0015400450045445U,
        0x4514155400554415U, 0x4555055051050151U, 0x1511441450001014U, 0x4544554510404414U,
        0x4115115545545450U, 0x5500541555551555U, 0x5550010544155015U, 0x0144414045545500U,
        0x4154050001050150U, 0x5550511111000145U, 0x1114504055000151U, 0x5104041101451040U,
        0x0010501401051441U, 0x0010501450504401U, 0x4554585440044444U, 0x5155555951450455U,
        0x0040000400105555U, 0x0000000000000001U
    };

    #endregion
}