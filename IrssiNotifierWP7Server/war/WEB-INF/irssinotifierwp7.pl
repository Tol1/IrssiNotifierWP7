#use strict;
#use warnings;

use Irssi;
use POSIX;
require CGI::Util;
use vars qw($VERSION %IRSSI);

$VERSION = "1";
%IRSSI = (
    authors     => "Tomi \'Tol1\' Nokkala, Lauri \'murgo\' Härsilä",
    contact     => "tol1\@iki.fi",
    name        => "IrssiNotifier for Windows Phone",
    description => "Send notifications about irssi highlights to server",
    license     => "Apache License, version 2.0",
    url         => "http://irssinotifierwp.appspot.com",
    changed     => "2012-04-10"
);

my $lastMsg;
my $lastServer;
my $lastNick;
my $lastAddress;
my $lastTarget;
my $lastKeyboardActivity = time;

sub private {
    my ($server, $msg, $nick, $address) = @_;
    $lastMsg = $msg;
    $lastServer = $server;
    $lastNick = $nick;
    $lastAddress = $address;
    $lastTarget = "PRIVATE";
}

sub public {
    my ($server, $msg, $nick, $address, $target) = @_;
    $lastMsg = $msg;
    $lastServer = $server;
    $lastNick = $nick;
    $lastAddress = $address;
    $lastTarget = $target;
}

sub print_text {
    my ($dest, $text, $stripped) = @_;

    my $opt = MSGLEVEL_HILIGHT|MSGLEVEL_MSGS;
    if (
        ($dest->{level} & ($opt)) && (($dest->{level} & MSGLEVEL_NOHILIGHT) == 0) &&
        (!Irssi::settings_get_bool("irssinotifierwp_away_only") || $lastServer->{usermode_away}) &&
        (!Irssi::settings_get_bool("irssinotifierwp_ignore_active_window") || ($dest->{window}->{refnum} != (Irssi::active_win()->{refnum}))) &&
        activity_allows_hilight()
    ) {
        hilite();
    }
}

sub activity_allows_hilight {
    my $timeout = Irssi::settings_get_int('irssinotifierwp_require_idle_seconds');
    return ($timeout <= 0 || (time - $lastKeyboardActivity) > $timeout);
}

sub dangerous_string {
  my $s = @_ ? shift : $_;
  return $s =~ m/"/ || $s =~ m/`/ || $s =~ m/\\/;
}

sub hilite {
    if (!Irssi::settings_get_str('irssinotifierwp_api_token')) {
        Irssi::print("IrssiNotifier: Set API token to send notifications: /set irssinotifierwp_api_token [token]");
        return;
    }

    `/usr/bin/env openssl version`;
    if ($? != 0) {
        Irssi::print("IrssiNotifier: You'll need to install OpenSSL to use IrssiNotifier");
        return;
    }

    `/usr/bin/env wget --version`;
    if ($? != 0) {
        Irssi::print("IrssiNotifier: You'll need to install Wget to use IrssiNotifier");
        return;
    }
    
    my $api_token = Irssi::settings_get_str('irssinotifierwp_api_token');
    if (dangerous_string $api_token) {
        Irssi::print("IrssiNotifier: Api token cannot contain backticks, double quotes or backslashes");
        return;
    }

#    my $encryption_password = Irssi::settings_get_str('irssinotifierwp_encryption_password');
#    if ($encryption_password) {
#        if (dangerous_string $encryption_password) {
#            Irssi::print("IrssiNotifier: Encryption password cannot contain backticks, double quotes or backslashes");
#            return;
#        }
#        $lastMsg = encrypt($lastMsg);
#        $lastNick = encrypt($lastNick);
#        $lastTarget = encrypt($lastTarget);
#    } else {
#        Irssi::print("IrssiNotifier: Set encryption password to send notifications (must be same as in the WP device): /set irssinotifierwp_encryption_password [password]");
#    }

#    $lastMsg = uri_escape($lastMsg);
#    $lastNick = uri_escape($lastNick);
#    $lastTarget = uri_escape($lastTarget);
    $lastMsg = CGI::Util::escape($lastMsg);
    $lastNick = CGI::Util::escape($lastNick);
    $lastTarget = CGI::Util::escape($lastTarget);

    my $data = "--post-data \"apiToken=$api_token\&message=$lastMsg\&channel=$lastTarget\&nick=$lastNick\&version=$VERSION\"";
#Irssi::print("`/usr/bin/env wget --no-check-certificate -qO- /dev/null $data https://irssinotifierwp.appspot.com/API/Message`");
    my $result = `/usr/bin/env wget --no-check-certificate -qO- /dev/null $data https://irssinotifierwp.appspot.com/API/Message`;
    if ($? != 0) {
        # Something went wrong, might be network error or authorization issue. Probably no need to alert user, though.
        # Irssi::print("IrssiNotifier: Sending hilight to server failed, check http://irssinotifierwp.appspot.com for updates");
        return;
    }
    
    if (length($result) > 0) {
        Irssi::print("IrssiNotifier: $result");
    }
}

sub sanitize {
    my $str = @_ ? shift : $_;
    $str =~ s/((?:^|[^\\])(?:\\\\)*)'/$1\\'/g;
    $str =~ s/\\'/´/g; # stupid perl
    $str =~ s/'/´/g; # stupid perl
    return "'$str'";
}

sub encrypt {
    my $text = $_[0];
    $text = sanitize $text;
    my $encryption_password = Irssi::settings_get_str('irssinotifierwp_encryption_password');
    my $result = `/usr/bin/env echo $text| /usr/bin/env openssl enc -aes-128-cbc -salt -base64 -A -k "$encryption_password" | tr -d '\n'`;
    $result =~ s/=//g;
    $result =~ s/\+/-/g;
    $result =~ s/\//_/g;
    chomp($result);
    return $result;
}

sub decrypt {
    my $text = $_[0];
    $text = sanitize $text;
    my $encryption_password = Irssi::settings_get_str('irssinotifierwp_encryption_password');
    my $result = `/usr/bin/env echo $text| /usr/bin/env openssl enc -aes-128-cbc -d -salt -base64 -A -k "$encryption_password"`;
    chomp($result);
    return $result;
}

sub setup_keypress_handler {
    Irssi::signal_remove('gui key pressed', 'event_key_pressed');
    if (Irssi::settings_get_int('irssinotifierwp_require_idle_seconds') > 0) {
        Irssi::signal_add('gui key pressed', 'event_key_pressed');
    }
}

sub event_key_pressed {
    $lastKeyboardActivity = time;
}

Irssi::settings_add_str('IrssiNotifierWP', 'irssinotifierwp_encryption_password', 'password');
Irssi::settings_add_str('IrssiNotifierWP', 'irssinotifierwp_api_token', '');
Irssi::settings_add_bool('IrssiNotifierWP', 'irssinotifierwp_away_only', false);
Irssi::settings_add_bool('IrssiNotifierWP', 'irssinotifierwp_ignore_active_window', false);
Irssi::settings_add_int('IrssiNotifierWP', 'irssinotifierwp_require_idle_seconds', 0);

Irssi::signal_add('message public', 'public');
Irssi::signal_add('message private', 'private');
Irssi::signal_add('print text', 'print_text');
Irssi::signal_add('setup changed', 'setup_keypress_handler');

setup_keypress_handler();