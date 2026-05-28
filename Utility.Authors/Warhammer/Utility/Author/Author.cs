using System.ComponentModel;
using UnityEngine;

namespace Warhammer.Utility.Author;

public enum Author
{
	[Description("ivanov")]
	[InspectorName("LD/Eugene Ivanov")]
	[DescriptionEmail("<ivanov@owlcat.games>")]
	EugeneIvanov = 1,
	[Description("polezhaev")]
	[InspectorName("LD/Alexey Polezhaev")]
	[DescriptionEmail("<polezhaev@owlcat.games>")]
	AlexeyPolezhaev = 5,
	[Description("zolotovsky")]
	[InspectorName("LD/Vyacheslav Zolotovsky")]
	[DescriptionEmail("<zolotovsky@owlcat.games>")]
	VyacheslavZolotovsky = 6,
	[Description("zuev")]
	[InspectorName("LD/Vladimir Zuev")]
	[DescriptionEmail("<zuev@owlcat.games>")]
	VladimirZuev = 7,
	[Description("schellenberg")]
	[InspectorName("LD/Artem Schellenberg")]
	[DescriptionEmail("<schellenberg@owlcat.games>")]
	ArtemSchellenberg = 10,
	[Description("kharybin")]
	[InspectorName("LD/Anton Kharybin")]
	[DescriptionEmail("<kharybin@owlcat.games>")]
	AntonKharybin = 13,
	[Description("rybolovlev")]
	[InspectorName("LD/Andrey Rybolovlev")]
	[DescriptionEmail("<rybolovlev@owlcat.games>")]
	AndreyRybolovlev = 14,
	[Description("konash")]
	[InspectorName("LD/Alexey Konash")]
	[DescriptionEmail("<konash@owlcat.games>")]
	AlexeyKonash = 15,
	[Description("e.shchur")]
	[InspectorName("LD/Eugene Schur")]
	[DescriptionEmail("<e.shchur@owlcat.games>")]
	EugeneSchur = 16,
	[Description("i.podberezin")]
	[InspectorName("LD/Ilya Podberezin")]
	[DescriptionEmail("<i.podberezin@owlcat.games>")]
	IlyaPodberezin = 17,
	[Description("a.serebryakov")]
	[InspectorName("LD/Alexandr Serebryakov")]
	[DescriptionEmail("<a.serebryakov@owlcat.games>")]
	AlexandrSerebryakov = 18,
	[Description("a.arkhipov")]
	[InspectorName("LD/Artem Arkhipov")]
	[DescriptionEmail("<a.arkhipov@owlcat.games>")]
	ArtemArkhipov = 19,
	[Description("a.gusev")]
	[InspectorName("GD/Alexander Gusev")]
	[DescriptionEmail("<a.gusev@owlcat.games>")]
	AlexanderGusev = 100,
	[Description("tolochenko")]
	[InspectorName("GD/Leonid Tolochenko")]
	[DescriptionEmail("<tolochenko@owlcat.games>")]
	LeonidTolochenko = 101,
	[Description("n.filatov")]
	[InspectorName("GD/Nikita Filatov")]
	[DescriptionEmail("<n.filatov@owlcat.games>")]
	NikitaFilatov = 102,
	[Description("doronin")]
	[InspectorName("GD/Georgii Doronin")]
	[DescriptionEmail("<doronin@owlcat.games>")]
	GeorgiiDoronin = 103,
	[Description("dzhalilov")]
	[InspectorName("GD/Amir Dzhalilov")]
	[DescriptionEmail("<dzhalilov@owlcat.games>")]
	AmirDzhalilov = 104,
	[Description("arhipovlinev")]
	[InspectorName("GD/Vasilii Arkhiplov-Linev")]
	[DescriptionEmail("<arhipovlinev@owlcat.games>")]
	VasiliiArkhiplovLinev = 105,
	[Description("belousov")]
	[InspectorName("GD/Maksim Belousov")]
	[DescriptionEmail("<belousov@owlcat.games>")]
	MaksimBelousov = 106,
	[Description("a.tumanov")]
	[InspectorName("GD/Aleksandr Tumanov")]
	[DescriptionEmail("<a.tumanov@owlcat.games>")]
	AleksandrTumanov = 107,
	[Description("e.vazhnichin")]
	[InspectorName("GD/Egor Vazhnichin")]
	[DescriptionEmail("<e.vazhnichin@owlcat.games>")]
	EgorVazhnichin = 108,
	[Description("a.gorelov")]
	[InspectorName("GD/Artem Gorelov")]
	[DescriptionEmail("<a.gorelov@owlcat.games>")]
	ArtemGorelov = 109,
	[Description("sverkunov")]
	[InspectorName("GD/Andrey Sverkunov")]
	[DescriptionEmail("<sverkunov@owlcat.games>")]
	AndreySverkunov = 110,
	[Description("k.goryachev")]
	[InspectorName("GD/Konstantin Goryachev")]
	[DescriptionEmail("<k.goryachev@owlcat.games>")]
	KonstantinGoryachev = 111,
	[Description("efanov")]
	[InspectorName("SD/Ilya Efanov")]
	[DescriptionEmail("<efanov@owlcat.games>")]
	IlyaEfanov = 201,
	[Description("boksha")]
	[InspectorName("SD/Artem Boksha")]
	[DescriptionEmail("<boksha@owlcat.games>")]
	ArtemBoksha = 202,
	[Description("filippov")]
	[InspectorName("SD/Denis Filippov")]
	[DescriptionEmail("<filippov@owlcat.games>")]
	DenisFilippov = 203,
	[Description("kuzenkov")]
	[InspectorName("SD/Konstantin Kuzenkov")]
	[DescriptionEmail("<kuzenkov@owlcat.games>")]
	KonstantinKuzenkov = 204,
	[Description("stepanyuk")]
	[InspectorName("SD/Aleksey Stepanyuk")]
	[DescriptionEmail("<stepanyuk@owlcat.games>")]
	AlekseyStepanyuk = 205,
	[Description("komovich")]
	[InspectorName("SD/Roman Komovich")]
	[DescriptionEmail("<komovich@owlcat.games>")]
	RomanKomovich = 206,
	[Description("sitdikov")]
	[InspectorName("SD/Rinat Sitdikov")]
	[DescriptionEmail("<r.sitdikov@owlcat.games>")]
	RinatSitdikov = 207,
	[Description("kellner")]
	[InspectorName("ND/Olga Kellner")]
	[DescriptionEmail("<kellner@owlcat.games>")]
	OlgaKellner = 302,
	[Description("larionova")]
	[InspectorName("ND/Veronika Larionova")]
	[DescriptionEmail("<larionova@owlcat.games>")]
	VeronikaLarionova = 303,
	[Description("d.koroleva")]
	[InspectorName("ND/Dinara Koroleva")]
	[DescriptionEmail("<d.koroleva@owlcat.games>")]
	DinaraKoroleva = 304,
	[Description("shabaev")]
	[InspectorName("ND/Aleksandr Shabaev")]
	[DescriptionEmail("<shabaev@owlcat.games>")]
	AleksandrShabaev = 305,
	[Description("fadeev")]
	[InspectorName("ND/Anton Fadeev")]
	[DescriptionEmail("<fadeev@owlcat.games>")]
	AntonFadeev = 306,
	[Description("beleckaya")]
	[InspectorName("ND/Margarita Beleckaya")]
	[DescriptionEmail("<beleckaya@owlcat.games>")]
	MargaritaBeleckaya = 307,
	[Description("peskov")]
	[InspectorName("CSD/Engeniy Peskov")]
	[DescriptionEmail("<peskov@owlcat.games>")]
	EngeniyPeskov = 401,
	[Description("bolshakov")]
	[InspectorName("CSD/Sergey Bolshakov")]
	[DescriptionEmail("<bolshakov@owlcat.games>")]
	SergeyBolshakov = 402,
	[Description("knyazev")]
	[InspectorName("FX/Kirill Knyasev")]
	[DescriptionEmail("<knyazev@owlcat.games>")]
	KirillKnyasev = 501,
	[Description("bodyagin")]
	[InspectorName("FX/Andrey Bodyagin")]
	[DescriptionEmail("<bodyagin@owlcat.games>")]
	AndreyBodyagin = 502,
	[Description("grudinin")]
	[InspectorName("FX/Ivan Grudinin")]
	[DescriptionEmail("<grudinin@owlcat.games>")]
	IvanGrudinin = 503,
	[Description("zhukov")]
	[InspectorName("CD/Aleksandr Zhukov")]
	[DescriptionEmail("<a.zhukov@owlcat.games>")]
	AleksandrZhukov = 601,
	[Description("chernyakov")]
	[InspectorName("CD/Aleksandr Chernyakov")]
	[DescriptionEmail("<chernyakov@owlcat.games>")]
	AleksandrChernyakov = 602,
	[Description("marinov")]
	[InspectorName("CD/Konstantin Marinov")]
	[DescriptionEmail("<marinov@owlcat.games>")]
	KonstantinMarinov = 603,
	[Description("sokolenko")]
	[InspectorName("CD/Aleksandr Sokolenko")]
	[DescriptionEmail("<sokolenko@owlcat.games>")]
	AleksandrSokolenko = 604,
	[Description("savenkov")]
	[InspectorName("CD/Maxim Savenkov")]
	[DescriptionEmail("<savenkov@owlcat.games>")]
	MaximSavenkov = 605,
	[Description("bulgakov")]
	[InspectorName("CD/Aleksei Bulgakov")]
	[DescriptionEmail("<vulgakov@owlcat.games>")]
	AlekseiBulgakov = 606,
	[Description("antipov")]
	[InspectorName("CD/Denis Antipov")]
	[DescriptionEmail("<antipov@owlcat.games>")]
	DenisAntipov = 607,
	[Description("bortyakov")]
	[InspectorName("CD/Ilya Bortyakov")]
	[DescriptionEmail("<i.bortyakov@owlcat.games>")]
	IlyaBortyakov = 608,
	[Description("morozov")]
	[InspectorName("CD/Anton Morozov")]
	[DescriptionEmail("<morozov@owlcat.games>")]
	AntonMorozov = 609,
	Unknown = -1
}
