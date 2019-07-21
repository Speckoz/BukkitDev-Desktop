﻿using BukkitDev_System._dep;
using BukkitDev_System._dep.MySQL;
using BukkitDev_System._dep.SQLite;
using BukkitDev_System._dep.XML;
using BukkitDev_System.Controles.Config;
using BukkitDev_System.Controles.Plugins.Plugin;
using BukkitDev_System.Controles.Subs;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace BukkitDev_System.Principal
{
	public partial class TelaInicial : Window
	{
		public static Snackbar barraDeNotificacao;
		public static DialogHost MensagemPerso;
		private readonly DispatcherTimer tema;
		private string temaAtual;

		[Obsolete]
		public TelaInicial()
		{
			InitializeComponent();
			//criando timer na inicializaçao programa
			tema = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(3)
			};
			tema.Tick += Tema_Tick;
			//
			barraDeNotificacao = BarraNotificacao_sb;
			MensagemPerso = MensagemDialog_dh;
		}
		#region Botoes do topo
		//minimiza a tela
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}
		//maximiza a tela
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			//se a condiçao for verdadeira, ou seja, se a tela estiver maximizada (fullScreen*) ao clicar a tela irá para o normal, senao, será maximizada
			WindowState = (Topmost = WindowState != WindowState.Maximized) ? WindowState.Maximized : WindowState.Normal;
		}

		private async void Button_Click_2(object sender, RoutedEventArgs e)
		{
			//verifica se realmente quer encerrar o programa 
			if (await EscolhaDialogHostAsync("Deseja encerrar o programa?"))
			{
				Application.Current.Shutdown();
			}
		}

		#endregion
		#region Utilitarios
		#region Carregamento Tela Principal
		[Obsolete]
		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//adiciona o nome que aparecerá na barra de tarefas, e tbm no topo da tela caso vc decida remover o topo personalizado {WindowStyle="None"}
			//PADRAO: BukkitDev + System
			//adiciona o nome do programa da textBlock no topo da tela
			TituloPrograma_txt.Text = Title = $"{PegarInfos.Nome} {PegarInfos.SobreNome}";
			//verificando se a janela está carregada
			if (IsLoaded)
			{
				//criando o xml caso ele nao exista
				await CriarLerXmlAsync();
				//verificando se arquivo do banco existe
				//e criando outro caso nao exista.
				await CriandoArquivoSQLiteAsync(new CriarBanco());
				//criar a tabela no banco, caso alguma nao exista...
				await new CriarTabela().CriarAsync();
				//setando seleçoes das configs mysql/ftp
				ConfiguMysqlFtp();
				//setando tema da config
				new TemaWindows().ConfigTemaPrograma(ref Light_mi, ref Dark_mi, ref PadraoWindows_mi);
				//setando cor da config
				ConfigCorPrograma();
				//setando valor da taxa na config
				ConfigTaxaEnvioPlugin();
				//setando valor do tamanho maximo do plugin permitido
				ConfigTamanhoMaxPlugin();
				//config imagem a ser usada
				ConfigImagem();
			}
		}

		private void ConfigImagem()
		{
			if (!string.IsNullOrEmpty(PegarInfos.ImagemPlugin))
			{
				bool re = PegarInfos.ImagemPlugin.Equals("true");
				EscolherImagemTipo_tb.IsChecked = re;
				EscolherImagemPadrao_st.IsEnabled = re;
			}
		}

		private void ConfigTamanhoMaxPlugin()
		{
			if (!string.IsNullOrEmpty(PegarInfos.TamanhoLimitePlugin.ToString()))
			{
				//alocando valor
				TamanhoInformado_txt.Text = PegarInfos.TamanhoLimitePlugin.ToString();
			}
		}
		private void ConfigTaxaEnvioPlugin()
		{
			if (!string.IsNullOrEmpty(PegarInfos.TaxaTransferencia.ToString()))
			{
				//alocando valor
				TaxaInformada_txt.Text = PegarInfos.TaxaTransferencia.ToString();
			}
		}

		[Obsolete]
		private void ConfigCorPrograma()
		{
			if (!string.IsNullOrEmpty(PegarInfos.Cor))
			{
				PaletteHelper palette = new PaletteHelper();
				//setando cor primaria
				palette.ReplacePrimaryColor(PegarInfos.Cor);
				//setando cor secundaria
				//por enquanto a cor secundaria pega é a mesma da primaria...
				palette.ReplaceAccentColor(PegarInfos.Cor);

				List<MenuItem> menus = new List<MenuItem>
					{
						LightBlue_mi,
						Purple_mi,
						Pink_mi,
						Green_mi,
						Red_mi
					};
				MenuItem cor = (PegarInfos.Cor == "LightBlue") ? menus[0] :
						 (PegarInfos.Cor == "Purple") ? menus[1] :
						 (PegarInfos.Cor == "Pink") ? menus[2] :
						 (PegarInfos.Cor == "Green") ? menus[3] :
						 (PegarInfos.Cor == "Red") ? menus[4] : null;
				foreach (MenuItem item in menus)
				{
					item.IsChecked = item == cor;
				}
			}
		}
		private void ConfiguMysqlFtp()
		{
			if (!EstaoVazios())
			{
				//verificando valores e setando de acordo com o arquivo xml
				//dados do mysql
				if (PegarInfos.ConfigMySQL == "Local")
				{
					LocalSelecionadoMySQL_mi.IsChecked = true;
					ExternoSelecionadoMySQL_mi.IsChecked = false;
				}
				else if (PegarInfos.ConfigMySQL == "Externo")
				{
					LocalSelecionadoMySQL_mi.IsChecked = false;
					ExternoSelecionadoMySQL_mi.IsChecked = true;
				}
				//dados do ftp
				if (PegarInfos.ConfigFTP == "Local")
				{
					LocalSelecionadoFTP_mi.IsChecked = true;
					ExternoSelecionadoFTP_mi.IsChecked = false;
				}
				else if (PegarInfos.ConfigFTP == "Externo")
				{
					LocalSelecionadoFTP_mi.IsChecked = false;
					ExternoSelecionadoFTP_mi.IsChecked = true;
				}
			}
		}
		private static bool EstaoVazios()
		{
			return string.IsNullOrEmpty(PegarInfos.ConfigMySQL) && string.IsNullOrEmpty(PegarInfos.ConfigFTP);
		}
		private static async Task CriandoArquivoSQLiteAsync(CriarBanco create)
		{
			create.CriarArquivo(PegarInfos.NomeArquivoSQLite);
			//verificando se a tabela nao existe e criando-a caso nao exista
			if (!await create.TabelaExisteAsync(PegarInfos.NomeArquivoSQLite))
			{
				await create.CriarTabelaAsync(PegarInfos.NomeArquivoSQLite);
			}
		}
		private static async Task CriarLerXmlAsync()
		{
			if (await CriandoArquivoXML.VerificarECriarAsync(PegarInfos.NomeArquivoXML))
			{
				//lendo dados do xml e guardando nas variaveis estaticas
				await MetodosConstantes.LerXMLAsync();
			}
		}
		#endregion
		[Obsolete]
		private void Tema_Tick(object sender, EventArgs e)
		{
			TemaWindows temaConfig = new TemaWindows();
			if (temaConfig.TemaClaroHabilitado().@string != temaAtual)
			{
				new PaletteHelper().SetLightDark(!temaConfig.TemaClaroHabilitado().@bool);
				//MetodosConstantes.EnviarMenssagem("foi mudado");
				temaAtual = temaConfig.TemaClaroHabilitado().@string;
			}
		}
		//mover a tela
		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				//se nao fizer essa verificaçao, irá ocorrer um erro caso aperte o botao direito do mouse
				//verifico se o botao que foi clicado é o esquerdo, e se isto for verdadeiro, entra no if
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					//se clicar duas vezes em cima do grid, mudará para Maximizado
					//MouseDoubleClick += TelaInicial_MouseDoubleClick;
					//se a janela estiver maximizada, é preciso colocar no estado normal, para entao poder mover.
					if (WindowState == WindowState.Maximized)
					{
						//setando o estado da jenela para o normal
						WindowState = WindowState.Normal;
						//setando a posiçao da janela em relaçao ao topo como 0
						Top = 0;
						//desativando o topMost para evitar bugs
						Topmost = false;
					}
					//com esse metodo, é possivel arrastar a janela na qual está o objeto, no caso, eu coloquei esse para quando ele disparar o evento mouseDown(clicou em cima) no grid do topo (que contem os botoes de fechar, minimizar...etc).
					DragMove();
				}
				else
				{
					//se a propriedade TopMost (sempre na frente) estiver ativa, a mesma é desativada
					if (Topmost)
					{
						Topmost = false;
						MetodosConstantes.EnviarMenssagem("Nao está mais fixado");
					}
					else
					{
						//senao, eh preciso verificar se o estado atual da janela é maximizado, ja que se eu tentar ficar uma janela sendo que ela esta maximizada nao tem sentido
						if (WindowState != WindowState.Maximized)
						{
							Topmost = true;
							MetodosConstantes.EnviarMenssagem("Agora está fixado");
						}
						else
						{
							MetodosConstantes.EnviarMenssagem("Voce nao pode fazer isto com a janela maximizada!");
						}
					}
				}
			}
			//isso nao é necessario, pois ele so entrará no if, se ele apertar o botao esquerdo, e como ele so dispara a exception se eu clicar em outro botao...isso é irrelevante!
			catch (InvalidOperationException erro)
			{
				MetodosConstantes.MostrarExceptions(erro);
			}
		}

		private void TelaInicial_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			WindowState = WindowState.Maximized;
		}

		//sobre o software
		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			_ = new Sobre().ShowDialog();
		}
		//avaliar software
		private void Button_Click_4(object sender, RoutedEventArgs e)
		{
			_ = Process.Start(PegarInfos.GitHubSourceLink);
		}
		#endregion
		#region CONFIGURAÇOES
		private async void MarcarClicadoAsync(string tipoEnvio, string tipoBanco, List<MenuItem> items, object sender)
		{
			foreach (MenuItem item in items)
			{
				item.IsChecked = item == (MenuItem)sender;
			}
			new AtualizandoDadosXML().AtualizarAsync(PegarInfos.NomeArquivoXML, tipoEnvio, tipoBanco);
			await MetodosConstantes.LerXMLAsync();
			MetodosConstantes.EnviarMenssagem($"{tipoEnvio} alterado para {tipoBanco}");
		}

		private void TipoMySQLLocal_Click(object sender, RoutedEventArgs e)
		{
			MarcarClicadoAsync("MySQL", "Local", new List<MenuItem> { ExternoSelecionadoMySQL_mi, LocalSelecionadoMySQL_mi }, LocalSelecionadoMySQL_mi);
		}

		private void TipoMySQLExterno_Click(object sender, RoutedEventArgs e)
		{
			MarcarClicadoAsync("MySQL", "Externo", new List<MenuItem> { ExternoSelecionadoMySQL_mi, LocalSelecionadoMySQL_mi }, ExternoSelecionadoMySQL_mi);
		}


		private void TipoFTPLocal_Click(object sender, RoutedEventArgs e)
		{
			MarcarClicadoAsync("FTP", "Local", new List<MenuItem> { ExternoSelecionadoFTP_mi, LocalSelecionadoFTP_mi }, LocalSelecionadoFTP_mi);
		}

		private void TipoFTPExterno_Click(object sender, RoutedEventArgs e)
		{
			MarcarClicadoAsync("FTP", "Externo", new List<MenuItem> { ExternoSelecionadoFTP_mi, LocalSelecionadoFTP_mi }, ExternoSelecionadoFTP_mi);
		}


		private void GravarTaxa_bt_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(TaxaInformada_txt.Text))
			{
				GravarAsync("TaxaDeTransferencia", TaxaInformada_txt.Text);
			}
		}

		private void GravarTamanho_bt_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(TamanhoInformado_txt.Text))
			{
				GravarAsync("TamanhoMaxPlugin", TamanhoInformado_txt.Text);
			}
		}

		private async void GravarAsync(string nomeXml, string valor)
		{
			try
			{
				//adicionando novo valor a taxa de transferencia
				new AtualizandoDadosXML().AtualizarAsync(PegarInfos.NomeArquivoXML, nomeXml, valor);
				//atualizando variaveis
				await MetodosConstantes.LerXMLAsync();

				//mostrando que foi enviado
				MetodosConstantes.EnviarMenssagem("Dados gravados com sucesso!");
			}
			catch (Exception erro)
			{
				MetodosConstantes.MostrarExceptions(erro);
			}
		}

		private readonly PaletteHelper Configs = new PaletteHelper();

		//mudando a thema do programa
		[Obsolete]
		private async void MudarTema_Click(object sender, RoutedEventArgs e)
		{
			//verificando se "quem" disparou o evento foi o menuItem do Light ou do Dark e guardando resultado em uma var
			bool result = (MenuItem)sender == Light_mi;
			//guardando string referente a "quem" disparou o evento
			string cor = result ? "Light" : "Dark";
			//mudando o estado (Checked) de acordo com o resultado da comparaçao acima
			Light_mi.IsChecked = result;
			Dark_mi.IsChecked = !result;
			//setando tema, lembrando que este metodo recebe TRUE como o dark, e como a comparaçao é com o light eu neguei o result
			Configs.SetLightDark(!result);
			//atualizando
			new AtualizandoDadosXML().AtualizarAsync(PegarInfos.NomeArquivoXML, "Tema", cor);
			await MetodosConstantes.LerXMLAsync();
			MetodosConstantes.EnviarMenssagem($"Tema do programa alterado para {cor}");
		}

		[Obsolete]
		private void PadraoWindows_mi_Click(object sender, RoutedEventArgs e)
		{
			if (PadraoWindows_mi.IsChecked)
			{
				DesabilitandoMenus();
				temaAtual = null;
				tema.Start();
			}
			else
			{
				Light_mi.IsEnabled = true;
				Dark_mi.IsEnabled = true;
				bool isDark = PegarInfos.Tema == "Dark";
				Light_mi.IsChecked = !isDark;
				Dark_mi.IsChecked = isDark;
				new PaletteHelper().SetLightDark(isDark);
				temaAtual = null;
				tema.Stop();
			}
		}

		private void DesabilitandoMenus()
		{
			Light_mi.IsEnabled = false;
			Light_mi.IsChecked = false;
			Dark_mi.IsEnabled = false;
			Dark_mi.IsChecked = false;
		}

		//mudando cor do programa
		[Obsolete]
		private async void SelecionarCorPrograma_Click(object sender, RoutedEventArgs e)
		{
			MenuItem select = (MenuItem)sender;

			List<MenuItem> menus = new List<MenuItem>
			{
				LightBlue_mi,
				Purple_mi,
				Pink_mi,
				Green_mi,
				Red_mi
			};

			//MenuItem i = e.Source as MenuItem;
			//MetodosConstantes.EnviarMenssagem(i.Header.ToString());

			foreach (MenuItem item in menus)
			{
				item.IsChecked = item == select;
			}

			//altera a cor
			await MudarCor.MudarAsync(CorRetornada(select));
		}
		private string CorRetornada(MenuItem select)
		{
			return (select == LightBlue_mi) ? "LightBlue" : (select == Purple_mi) ? "Purple" : (select == Pink_mi) ? "Pink" : (select == Green_mi) ? "Green" : (select == Red_mi) ? "Red" : string.Empty;
		}
		// ativar/desativar tool
		private void SelecionarToolPrograma_Click(object sender, RoutedEventArgs e)
		{
			bool @is = ((MenuItem)sender) == AtivarTool_mi;
			DesativarTool_mi.IsChecked = !@is;
			AtivarTool_mi.IsChecked = @is;
			Tool_bt.Visibility = @is ? Visibility.Visible : Visibility.Collapsed;
		}
		// ativar/desativar Menssagem
		private void MenssagemPrograma_Click(object sender, RoutedEventArgs e)
		{
			bool @is = ((MenuItem)sender) == AtivarMenssagem_mi;
			AtivarMenssagem_mi.IsChecked = @is;
			DesativarMenssagem_mi.IsChecked = !@is;
			barraDeNotificacao.IsEnabled = @is;
		}
		// ativar/desativar congiguraçao de imagem.
		private async void EscolherImagemTipo_tb_Click(object sender, RoutedEventArgs e)
		{
			if (((ToggleButton)sender).IsChecked.Equals(true))
			{
				new AtualizandoDadosXML().AtualizarAsync(PegarInfos.NomeArquivoXML, "ImagemPlugin", "true");
				EscolherImagemPadrao_st.IsEnabled = true;
			}
			else
			{
				new AtualizandoDadosXML().AtualizarAsync(PegarInfos.NomeArquivoXML, "ImagemPlugin", "false");
				EscolherImagemPadrao_st.IsEnabled = false;
			}
			await MetodosConstantes.LerXMLAsync();
			MetodosConstantes.EnviarMenssagem("Configuraçao de imagem selecionada foi alterada!");
		}
		#endregion
		#region Controles de Usuarios
		private void AdicionarNovoUserControl(UIElement uControl)
		{
			//limpa tudo de dentro do grid
			//ControlesDeTela_gd.Children.Clear();
			ControlesDeTela_sv.Content = uControl;
			//adiciona um novo elemento, que no caso é um UserControl
			//ControlesDeTela_gd.Children.Add(uControl);
		}

		//itens
		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			AdicionarNovoUserControl(new NovoPlugin());
		}
		private void MenuItem_Click_3(object sender, RoutedEventArgs e)
		{
			AdicionarNovoUserControl(new ListarPlugins());
		}
		private void MenuItem_Click_4(object sender, RoutedEventArgs e)
		{
			AdicionarNovoUserControl(new Controles.Plugins.Licenca.AdicionarLicenca());
		}
		private void MenuItem_Click_5(object sender, RoutedEventArgs e)
		{
			AdicionarNovoUserControl(new Controles.Plugins.Licenca.ListarLicenca());
		}
		private void MenuItem_Click_6(object sender, RoutedEventArgs e)
		{
			AdicionarNovoUserControl(new Controles.Plugins.Plugin.RemoverPlugin());
		}
		//configuraçao
		private void MenuItem_Click_1(object sender, RoutedEventArgs e)
		{
			AdicionarNovoUserControl(new MySQLConfig(PegarInfos.ConfigMySQL));
		}
		private void MenuItem_Click_2(object sender, RoutedEventArgs e)
		{
			AdicionarNovoUserControl(new FTPConfig(PegarInfos.ConfigFTP));
		}
		#endregion
		#region ignorar
		public static async void EnviarMensagemDialogHostAsync(string msg)
		{
			try
			{
				_ = await DialogHost.Show(new DialogHostSimples { Mensagem_txt = { Text = msg } }, "RootDialog");
			}
			catch
			{
				MetodosConstantes.EnviarMenssagem("Ouve um problema na inicializaçao");
			}
		}
		public static async Task<bool> EscolhaDialogHostAsync(string msg)
		{
			try
			{
				DialogHostEscolha esc = new DialogHostEscolha { Mensagem_txt = { Text = msg } };
				_ = await DialogHost.Show(esc, "RootDialog");

				return esc.clicouAceitar;
			}
			catch
			{
				MetodosConstantes.EnviarMenssagem("Erro ao escolher uma opçao");
				return false;
			}
		}
		//testes
		//private void Window_MouseMove(object sender, MouseEventArgs e)
		//{
		//	Cursor = e.GetPosition(this).X <= Width && e.GetPosition(this).X > Width - 6 ? Cursors.ScrollWE : Cursors.Arrow;
		//}
		#endregion
	}
}
