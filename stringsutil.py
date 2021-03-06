# -*- coding: utf-8 -*-
"""
Created on Fri Dec 13 12:40:23 2019

@author: millan
"""

#!/usr/bin/env python

import re, datetime, argparse, sys, os

class StringResGenerator(object):
    ## global config args
    _languages = []
    _spliter = '='
    _ns = ''
    _ns_uri = ''
    _ns_pattern = ''
    _default_dict = ''
    _dict_pattern = ''
    _str_pattern = ''
    _extra_line = False
    _files = []
    _outdir = ''
    _generate_dict = ''

    def __init__(self, languages=[], spliter='=', ns='', ns_uri='', ns_pattern='', default_dict='', dict_pattern='', str_pattern='', files=[], extra_line=False, outdir='', generate_dict=''):
        self._languages = languages
        self._spliter = spliter
        self._ns = ns
        self._ns_uri = ns_uri
        self._ns_pattern = ns_pattern
        self._default_dict = default_dict
        self._dict_pattern = dict_pattern
        self._str_pattern = str_pattern
        self._files = files
        self._extra_line = extra_line
        self._outdir = outdir
        self._generate_dict = generate_dict

    def verify_args(self):
        if not self._ns_uri and not self._ns:
            print('either ns or usuri must be set!')
            return -1
        if not self._str_pattern:
            print('key-pattern is not set! Try to use predefined config "lex" or "loc" to have a predefined key-pattern')
            return -1
        if len(self._files) == 0 and not self._generate_dict:
            print("no xaml files or generated dictionary provided")
            return -1
        if not self._default_dict and not self._generate_dict:
            print('default_dict is empty, set to DefaultDict in case the key has no dictionary detected')
            self._default_dict = 'DefaultDict'
        return 0

    def process(self):
        if not not self._generate_dict:
            generate_dictionary(self._outdir, self._generate_dict, self._languages)
            return
        nspat = re.compile(self._ns_pattern % to_re_pattern(self._ns_uri)) if self._ns_uri else ''
        results = dict()
        for file in self._files:
            r = read_keys(file, self._ns, nspat, self._default_dict, self._dict_pattern, self._str_pattern)
            for dict_ in r.keys():
                if dict_ in results:
                    results[dict_] = results[dict_].union(r[dict_])
                else:
                    results[dict_] = r[dict_]
        write_keys(results, self._languages, self._spliter, self._extra_line, self._outdir)


def generate_dictionary(outdir, dict_, langs):
    for lang in langs:
        filename = make_filename(outdir, dict_, lang)
        if not os.path.exists(filename):
            with open(filename, "w") as f:
                print('create dictionary %s' % filename)
                f.write('## Created by %s at %s\n' % (__file__, str(datetime.datetime.now())))


def to_re_pattern(uri):
    return uri.replace('.', r'\.')


def make_filename(outdir, key, lang):
    return os.path.join(outdir, '%s%s.txt' % (key, '.' + lang if lang else ''))


def filter_keys(fn, spliter):
    lines = []
    try:
        with open(fn, 'r') as f:
            lines = f.readlines()
    except:
        pass
    exist_keys = set()
    for line in lines:
        if line.strip() and line[0] != '#':
            key = line.split(spliter)[0]
            exist_keys.add(key)
    return exist_keys


def read_keys(xaml_fn, ns, nspat, default_dict, dictpattern, strpattern):
    alltext = ''
    with open(xaml_fn, 'r') as f:
        alltext = ''.join(f.readlines())

    ## first: find the namespace ns    
    if not ns:
        match = nspat.search(alltext)
        if match is not None:
            ns = match.group(1)
    
    if not ns:
        print('ns NOT found in %s' % xaml_fn)
        return {}

    print('ns found in %s: %s' % (xaml_fn, ns))

    ## second: find default dictionary
    if not not dictpattern:
        dictpat = re.compile(dictpattern % ns)
        match = dictpat.search(alltext)
        if match is not None:
            default_dict = match.group(1)

    print('default dictionary found in %s: %s' % (xaml_fn, default_dict))

    ## third: find all matching texts
    strpat = re.compile(strpattern % ns)
    results = strpat.findall(alltext)
    
    ## forth: convert to dictionary
    dicts = dict()
    for res in results:
        dict_ = res[0] if res[0] else default_dict
        if dict_ in dicts:
            dicts[dict_].add(res[1].strip())
        else:
            dicts[dict_] = set([res[1].strip()])
    return dicts


def write_keys(dicts, langs, spliter, extra_line, outdir):
    ## fifth, write result to dictionary file
    for lang in langs:
        for dict_ in dicts.keys():
            fn = make_filename(outdir, dict_, lang)
            exist_keys = filter_keys(fn, spliter)
            new_keys = dicts[dict_]
            if exist_keys.issuperset(new_keys):
                continue
            print('write dictionary %s' % fn)
            with open(fn, 'a') as f:
                f.write('\n## Auto-generated by %s at %s\n\n' % (__file__, str(datetime.datetime.now())))
                for key in new_keys:
                    if not key in exist_keys:
                        f.write(key + '=\n' + ('\n' if extra_line else ''))
                        exist_keys.add(key)


def parse_args(args):
    parser = argparse.ArgumentParser()
    parser.add_argument('-c', '--config', choices=['lex', 'loc'], default='lex', help='use a predefined configuration, default is \'lex\'')
    parser.add_argument('-n', '--ns', default='', help='alias of namespace, either ns or nsuri must be provided')
    parser.add_argument('-N', '--nsuri', default='', help='uri of namespace, either ns or nsuri must be provided')
    parser.add_argument('-p', '--nspattern', default=r'\s+xmlns:(\w+)="%s"', help=r'regular expr pattern to find the alias of namespace, default is ''\s+xmlns:(\w+)="%%s"''')
    parser.add_argument('-d', '--default-dict', default='', help='default dictionary(resources class) name for the strings')
    parser.add_argument('-D', '--dict-pattern', default='', help='regular expr pattern to find the default dictionary in the xaml')
    parser.add_argument('-k', '--key-pattern', default='', help='regular expr pattern to find the key (and the dictionary) in the xaml')
    parser.add_argument('-l', '--lang', default='zh-CN,en-US', help='languages for files to generate. default is \'zh-CN,en-US\'')
    parser.add_argument('-s', '--spliter', default='=', help='split character in the output files, defulat is \'=\'')
    parser.add_argument('-e', '--extra-line', default=False, action='store_true', help='Add extra line after output each string key line, default is False')
    parser.add_argument('-o', '--outdir', default='', help='output dir')
    parser.add_argument('-g', '--generate-dict', required=False, help='generate dictionary directly')
    parser.add_argument('xaml_paths', nargs='*', metavar='SourceFile(s)', help='xaml files to parse or folder in which to parse all the xaml files')
    return parser.parse_args(args)


def dir_xamls(paths, ext='.xaml'):
    files = []
    for pa in paths:
        if os.path.isdir(pa):
            for dirpath, dirnames, filenames in os.walk(pa):
                for filename in filenames:
                    if filename[-len(ext):].lower() == ext.lower():
                        file = os.path.join(dirpath, filename)
                        files.append(file)
                        print('add %s to process' % file)
        else:
            files.append(pa)
            print('add %s to process' % pa)
    print('');
    return files


## regular expr config
predefined_config = dict()
predefined_config['lex'] =\
{
 'uri': r'http://wpflocalizeextension.codeplex.com',
 'dict_pattern': r'\s+%s:ResxLocalizationProvider\.DefaultDictionary="(\w+)"',
 'key_pattern': r'\{\s*%s:Loc\s+(?:Key=)?(?:(\w*):){0,2}(\S+)\s*\}'
}

predefined_config['loc'] =\
{
 'uri': '',
 'dict_pattern': '',
 'key_pattern': r'\{\s*x:Static\s+%s:(\w*)\.(\S+)\s*\}'
}

if __name__ == '__main__':
    args = parse_args(sys.argv[1:])
    config = predefined_config[args.config] if args.config else None
    if config:
        nsuri = config['uri']
        dictpattern = config['dict_pattern']
        strpattern = config['key_pattern']
    
    nsuri = args.nsuri if args.nsuri else nsuri
    dictpattern = args.dict_pattern if args.dict_pattern else dictpattern
    strpattern = args.key_pattern if args.key_pattern else strpattern
    
    ns = args.ns
    nspattern = args.nspattern
    default_dict = args.default_dict
    extra_line = args.extra_line
    languages = args.lang.split(',')
    if '' not in languages:
        languages.append('')
    spliter = args.spliter[0] if args.spliter else '='
    paths = dir_xamls(args.xaml_paths)
    outdir = args.outdir
    generate_dict = args.generate_dict
    gen = StringResGenerator(languages, spliter, ns, nsuri, nspattern, default_dict, dictpattern, strpattern, paths, extra_line, outdir, generate_dict)
    if gen.verify_args() == -1:
        exit(0)
    gen.process()
